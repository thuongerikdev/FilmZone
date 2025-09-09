using FilmZone.Middlewares;
using FZ.Auth.ApplicationService.StartUp;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

namespace FZ.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load env
            builder.Configuration.AddEnvironmentVariables();
            try
            {
                var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
                if (File.Exists(envPath)) DotNetEnv.Env.Load(envPath);
            }
            catch { }

            // ===== KESTREL: HTTP ONLY trên Fly (Production), Dev thì giữ cấu hình HTTPS nếu có =====
            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    // cho phép dùng endpoints trong appsettings.Development.json (nếu có)
                    options.Configure(context.Configuration.GetSection("Kestrel"));
                }
                else
                {
                    // ép chỉ nghe HTTP:8080 trong container
                    options.ListenAnyIP(8080);
                }
            });

            // Modules & Services
            builder.ConfigureAuth(typeof(Program).Namespace);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Redis
            builder.Services.AddStackExchangeRedisCache(opt =>
            {
                // Env: Redis__ConnectionString => "Redis:ConnectionString"
                opt.Configuration = builder.Configuration["Redis:ConnectionString"];
                opt.InstanceName = "FilmZone";
            });

            // CORS cho FE
            builder.Services.AddCors(opt =>
            {
                var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                               ?? new[] { builder.Configuration["Frontend:AppUrl"] ?? "http://localhost:5173" };
                opt.AddPolicy("FE", p => p.WithOrigins(origins)
                                          .AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .AllowCredentials());
            });

            // Swagger
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "FilmZone API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter JWT like: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Nhận X-Forwarded-* từ Fly edge
            var fwd = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fwd.KnownNetworks.Clear();
            fwd.KnownProxies.Clear();
            app.UseForwardedHeaders(fwd);

            // KHÔNG redirect HTTPS trong container (Fly đã terminate TLS bên ngoài)
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseCors("FE");

            // Map cookie -> Authorization header
            app.UseMiddleware<CookieJwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
