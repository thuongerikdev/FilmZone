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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading.Channels;

namespace FZ.Movie.ApplicationService.StartUp
{
    public static class MovieStartUp
    {
        public static void ConfigureMovie(this WebApplicationBuilder builder, string? assemblyName)
        {
            var services = builder.Services;
            var config = builder.Configuration;

            // Redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var cs = config["Redis:ConnectionString"] ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(cs);
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
