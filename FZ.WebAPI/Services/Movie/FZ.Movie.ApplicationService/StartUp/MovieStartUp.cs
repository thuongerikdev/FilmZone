using CloudinaryDotNet;
using FZ.Constant.Database;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.ApplicationService.Service.Implements;
using FZ.Movie.ApplicationService.Service.Implements.Catalog;
using FZ.Movie.ApplicationService.Service.Implements.Interactions;
using FZ.Movie.ApplicationService.Service.Implements.Media;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source.Archive;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source.Vimeo;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source.Youtube;
using FZ.Movie.ApplicationService.Service.Implements.People;
using FZ.Movie.ApplicationService.Service.Implements.Taxonomy;
using FZ.Movie.Infrastructure;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Catalog;
using FZ.Movie.Infrastructure.Repository.Interactions;
using FZ.Movie.Infrastructure.Repository.Media;
using FZ.Movie.Infrastructure.Repository.People;
using FZ.Movie.Infrastructure.Repository.Taxonomy;
using FZ.Shared.ApplicationService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Net;
using System.Threading.Channels;

namespace FZ.Movie.ApplicationService.StartUp
{
    public static class MovieStartUp
    {
        public static void ConfigureMovie(this WebApplicationBuilder builder, string? assemblyName)
        {
            var services = builder.Services;
            var config = builder.Configuration;


            // Program.cs
            builder.Services.AddHttpClient("archive", c =>
            {
                c.BaseAddress = new Uri("https://s3.us.archive.org");
                c.DefaultRequestHeaders.UserAgent.ParseAdd("FilmZoneUploader/1.0");
                c.Timeout = Timeout.InfiniteTimeSpan; // 👈 tránh timeout mặc định 100s
            });


            // === Redis ===
            // === Redis ===
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Redis");
                var config = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

                // Try parse as URI first (handle rediss://...)
                string host = null!;
                int port = 6379;
                string? user = null;
                string? password = null;
                var isTls = false;

                if (Uri.TryCreate(config, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "redis" || uri.Scheme == "rediss"))
                {
                    isTls = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase);
                    host = uri.Host;
                    port = uri.Port > 0 ? uri.Port : (isTls ? 6379 : 6379);
                    if (!string.IsNullOrEmpty(uri.UserInfo))
                    {
                        var parts = uri.UserInfo.Split(':', 2);
                        if (parts.Length >= 1) user = parts[0];
                        if (parts.Length == 2) password = parts[1];
                    }
                }
                else
                {
                    // fallback: try parse simple "host:port,option=..."
                    // Use ConfigurationOptions.Parse to pick up options in that case
                    var tmp = ConfigurationOptions.Parse(config);
                    // If Parse gave us a DnsEndPoint, use it; otherwise we will let Parse handle it
                    if (tmp.EndPoints.Count == 1 && tmp.EndPoints[0] is DnsEndPoint dns)
                    {
                        host = dns.Host;
                        port = dns.Port;
                    }
                    // Also copy password/user if present
                    if (!string.IsNullOrEmpty(tmp.Password)) password = tmp.Password;
                    if (!string.IsNullOrEmpty(tmp.User)) user = tmp.User;
                    isTls = tmp.Ssl;
                }

                // Build clean ConfigurationOptions to avoid accidental "full-URI as endpoint" bugs
                var options = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    ResolveDns = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 30000,
                    SyncTimeout = 30000,
                    KeepAlive = 10,
                    ClientName = "auth-service",
                };

                // correctly add endpoint
                if (!string.IsNullOrEmpty(host))
                {
                    options.EndPoints.Add(host, port);
                }
                else
                {
                    // fallback to parsing whole config if host parsing failed
                    options = ConfigurationOptions.Parse(config);
                }

                // TLS / SNI
                if (isTls)
                {
                    options.Ssl = true;
                    options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    // Ensure SslHost is only the host (no scheme/userinfo)
                    if (!string.IsNullOrEmpty(host)) options.SslHost = host;
                }

                // set auth if available
                if (!string.IsNullOrEmpty(user)) options.User = user;
                if (!string.IsNullOrEmpty(password)) options.Password = password;

                logger.LogInformation("Redis config: host={host} port={port} ssl={ssl} user={userPresent}", host, port, options.Ssl, string.IsNullOrEmpty(options.User) ? "(none)" : "(present)");

                try
                {
                    // connect (use async connect but wait — longer timeouts above help)
                    var mux = ConnectionMultiplexer.ConnectAsync(options).GetAwaiter().GetResult();

                    // optional: do not always PING in prod; but in dev it's helpful
                    try
                    {
                        var ping = mux.GetDatabase().Ping();
                        logger.LogInformation("Redis PING = {ms} ms", ping.TotalMilliseconds);
                    }
                    catch (Exception pingEx)
                    {
                        logger.LogWarning(pingEx, "Redis PING failed (multiplexer exists and will retry in background)");
                    }

                    return mux;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Redis Connect failed (config prefix: {prefix})", (config ?? "").Split('@').FirstOrDefault());
                    throw;
                }
            });


            builder.Services.Configure<FormOptions>(o => {
                o.MultipartBodyLengthLimit = 256L * 1024L * 1024L; // 256MB
            });


            // DbContext (Scoped)
            services.AddDbContext<MovieDbContext>(options =>
            {
                options.UseSqlServer(
                    config.GetConnectionString("Default"),
                    sql =>
                    {
                        if (!string.IsNullOrWhiteSpace(assemblyName))
                            sql.MigrationsAssembly(assemblyName);

                        sql.MigrationsHistoryTable(DbSchema.TableMigrationsHistory, DbSchema.Movie);
                    });
            });

            // SignalR + CORS (default policy allow any origin/method/header + credentials)
            services.AddSignalR();
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(p => p
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true)
                );
            });




            // HttpClient
            services.AddHttpClient("vimeo-api", c =>
            {
                c.BaseAddress = new Uri("https://api.vimeo.com/");
            });
            services.AddHttpClient(); // generic client (VD: TUS PATCH…)

            // Cloudinary (Singleton)
            var cloudSection = config.GetSection("Cloudinary");
            var cloudName = cloudSection["CloudName"];
            var apiKey = cloudSection["ApiKey"];
            var apiSecret = cloudSection["ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing or invalid (CloudName/ApiKey/ApiSecret).");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            services.AddSingleton(cloudinary);

            // Channel hàng đợi upload (Singleton)
            var channel = Channel.CreateUnbounded<UploadWorkItem>();
            services.AddSingleton(channel);
            services.AddSingleton<ChannelReader<UploadWorkItem>>(channel);
            services.AddSingleton<ChannelWriter<UploadWorkItem>>(channel);

            // Các Provider upload KHÔNG dùng DbContext trực tiếp => Singleton OK
            services.AddSingleton<IVideoUploadProvider, VimeoTusUploadProvider>();
            services.AddSingleton<IVideoUploadProvider, VimeoPullUploadProvider>();
            services.AddSingleton<IVideoUploadProvider, YouTubeResumableProvider>();
            services.AddSingleton<IVideoUploadProvider, InternetArchiveS3FileProvider>();
            services.AddSingleton<IVideoUploadProvider, InternetArchiveS3LinkProvider>();

            // Resolver provider (Singleton)
            services.AddSingleton<ProviderResolver>();

            // Repositories (Scoped)
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IEpisodeRepository, EpisodeRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IWatchProgressRepository, WatchProgressRepository>();
            services.AddScoped<IUserRatingRepository, UserRatingRepository>();
            services.AddScoped<IEpisodeWatchProgressRepository, EpisodeWatchingProgressRepository>();
            services.AddScoped<ISavedMovieRepository, SavedMovieRepository>();
            services.AddScoped<IEpisodeSourceRepository, EpisodeSourceRepository>();
            services.AddScoped<IImageSourceRepository, ImageSourceRepository>();
            services.AddScoped<IMovieImageRepository, MovieImageRepository>();
            services.AddScoped<IMovieSourceRepository, MovieSourceRepository>();
            services.AddScoped<IMoviePersonRepository, MoviePersonRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IMovieTagRepository, MovieTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IRegionRepository, RegionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services (Scoped)
            services.AddScoped<IMoviesService, MoviesService>();
            services.AddScoped<IEpisodeService, EpisodeService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IWatchProgressService, WatchProgressService>();
            services.AddScoped<IUserRatingService, UserRatingService>();
            services.AddScoped<IEpisodeWatchProgressService, EpisodeWatchProgressService>();
            services.AddScoped<ISavedMovieService, SavedMovieService>();
            services.AddScoped<IEpisodeSourceService, EpisodeSourceService>();
            services.AddScoped<IImageSourceService, ImageSourceService>();
            // services.AddScoped<IMovieImageService, MovieImageService>();
            services.AddScoped<IMovieSourceService, MovieSourceService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IMoviePersonService, MoviePersonService>();
            services.AddScoped<IMovieTagService, MovieTagService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IRegionService, RegionService>();

            // Cloudinary wrapper (Scoped) nếu bạn có service riêng
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Background worker — CHỈ đăng ký 1 lần
            services.AddHostedService<UploadCoordinator>();
        }
    }
}
