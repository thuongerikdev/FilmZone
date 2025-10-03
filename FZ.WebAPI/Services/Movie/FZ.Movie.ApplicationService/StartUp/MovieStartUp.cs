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
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Channels;

namespace FZ.Movie.ApplicationService.StartUp
{
    public static class MovieStartUp
    {
        // Convert URL (postgresql://…) -> KV-form cho Npgsql; KV-form thì chỉ đảm bảo SSL Mode/Trust.
        private static string NormalizePg(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            raw = raw.Trim();

            bool IsUrl(string s) =>
                s.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

            if (IsUrl(raw))
            {
                var uri = new Uri(raw);

                string? user = null, pass = null;
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var parts = uri.UserInfo.Split(':', 2);
                    user = Uri.UnescapeDataString(parts[0]);
                    if (parts.Length == 2) pass = Uri.UnescapeDataString(parts[1]);
                }

                var db = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
                var port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;

                var qs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = pair.Split('=', 2);
                    var k = Uri.UnescapeDataString(kv[0]);
                    var v = kv.Length == 2 ? Uri.UnescapeDataString(kv[1]) : "";
                    qs[k] = v;
                }

                var sslMode = qs.TryGetValue("sslmode", out var s) ? s : "require";
                var channel = qs.TryGetValue("channel_binding", out var cb) ? cb : null;

                var sb = new StringBuilder();
                sb.Append($"Host={uri.Host};Port={port};Database={db};Username={user};");
                if (!string.IsNullOrEmpty(pass)) sb.Append($"Password={pass};");
                sb.Append($"SSL Mode={sslMode};Trust Server Certificate=true;");
                if (!string.IsNullOrEmpty(channel)) sb.Append($"Channel Binding={channel};");
                return sb.ToString();
            }

            if (!raw.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
                raw += (raw.EndsWith(";") ? "" : ";") + "SSL Mode=Require";
            if (!raw.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
                raw += (raw.EndsWith(";") ? "" : ";") + "Trust Server Certificate=true";

            return raw;
        }

        public static void ConfigureMovie(this WebApplicationBuilder builder, string? assemblyName)
        {
            var services = builder.Services;
            var config = builder.Configuration;

            // HttpClient Archive (không timeout)
            builder.Services.AddHttpClient("archive", c =>
            {
                c.BaseAddress = new Uri("https://s3.us.archive.org");
                c.DefaultRequestHeaders.UserAgent.ParseAdd("FilmZoneUploader/1.0");
                c.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            });

            // === Redis ===
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Redis");
                var redisConnStr = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

                string host = null!;
                int port = 6379;
                string? user = null;
                string? password = null;
                var isTls = false;

                if (Uri.TryCreate(redisConnStr, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "redis" || uri.Scheme == "rediss"))
                {
                    isTls = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase);
                    host = uri.Host;
                    port = uri.Port > 0 ? uri.Port : 6379;
                    if (!string.IsNullOrEmpty(uri.UserInfo))
                    {
                        var parts = uri.UserInfo.Split(':', 2);
                        if (parts.Length >= 1) user = parts[0];
                        if (parts.Length == 2) password = parts[1];
                    }
                }
                else
                {
                    var tmp = ConfigurationOptions.Parse(redisConnStr);
                    if (tmp.EndPoints.Count == 1 && tmp.EndPoints[0] is DnsEndPoint dns)
                    {
                        host = dns.Host; port = dns.Port;
                    }
                    if (!string.IsNullOrEmpty(tmp.Password)) password = tmp.Password;
                    if (!string.IsNullOrEmpty(tmp.User)) user = tmp.User;
                    isTls = tmp.Ssl;
                }

                var options = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    ResolveDns = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 30000,
                    SyncTimeout = 30000,
                    KeepAlive = 10,
                    ClientName = "movie-service",
                };

                if (!string.IsNullOrEmpty(host)) options.EndPoints.Add(host, port);
                else options = ConfigurationOptions.Parse(redisConnStr);

                if (isTls)
                {
                    options.Ssl = true;
                    options.SslProtocols = SslProtocols.Tls12;
                    if (!string.IsNullOrEmpty(host)) options.SslHost = host;
                }

                if (!string.IsNullOrEmpty(user)) options.User = user;
                if (!string.IsNullOrEmpty(password)) options.Password = password;

                logger.LogInformation("Redis config: host={host} port={port} ssl={ssl} user={userPresent}",
                    host, port, options.Ssl, string.IsNullOrEmpty(options.User) ? "(none)" : "(present)");

                try
                {
                    var mux = ConnectionMultiplexer.ConnectAsync(options).GetAwaiter().GetResult();

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
                    logger.LogError(ex, "Redis Connect failed (config prefix: {prefix})", (redisConnStr ?? "").Split('@').FirstOrDefault());
                    throw;
                }
            });

            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 256L * 1024L * 1024L; // 256MB
            });

            // === DbContext (PostgreSQL/Neon) ===
            services.AddDbContext<MovieDbContext>(options =>
            {
                var raw =
                    config.GetConnectionString("Default") ??
                    Environment.GetEnvironmentVariable("DATABASE_URL") ??
                    throw new InvalidOperationException("Missing Postgres connection string");

                var conn = NormalizePg(raw);

                options.UseNpgsql(conn, npg =>
                {
                    if (!string.IsNullOrWhiteSpace(assemblyName))
                        npg.MigrationsAssembly(assemblyName);

                    npg.MigrationsHistoryTable(DbSchema.TableMigrationsHistory, DbSchema.Movie);
                    npg.EnableRetryOnFailure();
                });

                // options.UseSnakeCaseNamingConvention(); // nếu muốn
            });

            // SignalR + CORS
            services.AddSignalR();
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(p => p
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true));
            });

            // HttpClients khác
            services.AddHttpClient("vimeo-api", c => { c.BaseAddress = new Uri("https://api.vimeo.com/"); });
            services.AddHttpClient();

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

            // Các Provider upload
            services.AddSingleton<IVideoUploadProvider, VimeoTusUploadProvider>();
            services.AddSingleton<IVideoUploadProvider, VimeoPullUploadProvider>();
            services.AddSingleton<IVideoUploadProvider, YouTubeResumableProvider>();
            services.AddSingleton<IVideoUploadProvider, InternetArchiveS3FileProvider>();
            services.AddSingleton<IVideoUploadProvider, InternetArchiveS3LinkProvider>();

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
            services.AddScoped<IMovieSourceService, MovieSourceService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IMoviePersonService, MoviePersonService>();
            services.AddScoped<IMovieTagService, MovieTagService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IRegionService, RegionService>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Background worker
            services.AddHostedService<UploadCoordinator>();
        }
    }
}
