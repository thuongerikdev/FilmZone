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

            // 1) Ưu tiên biến môi trường (Fly secrets)
            builder.Configuration.AddEnvironmentVariables();

            // 2) Chỉ load .env khi chạy local (dev)
            try
            {
                var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
                if (builder.Environment.IsDevelopment() && File.Exists(envPath))
                    DotNetEnv.Env.Load(envPath);
            }
            catch { /* ignore */ }

            // 3) Kestrel: Prod (Fly) chỉ nghe HTTP:8080; Dev thì dùng cấu hình trong appsettings.Development.json
            builder.WebHost.ConfigureKestrel((ctx, opt) =>
            {
                if (ctx.HostingEnvironment.IsDevelopment())
                {
                    // Dev muốn lấy từ appsettings.Development.json thì giữ nguyên
                    opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                }
                else
                {
                    // Prod: HTTP only trên cổng 8080 trong container
                    opt.ListenAnyIP(8080);
                }
            });

          


            // 4) Đăng ký modules & services
            builder.ConfigureAuth(typeof(Program).Namespace);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Redis
            builder.Services.AddStackExchangeRedisCache(o =>
            {
                // Map từ Fly secrets: Redis__ConnectionString -> "Redis:ConnectionString"
                o.Configuration = builder.Configuration["Redis:ConnectionString"];
                o.InstanceName = "FilmZone";
            });

            // CORS cho FE
            builder.Services.AddCors(opt =>
            {
                var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? new[] { builder.Configuration["Frontend:AppUrl"] ?? "http://localhost:3000" };

                opt.AddPolicy("FE", p => p
                    .WithOrigins(origins)
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

            // Prod KHÔNG redirect HTTPS trong container
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Swagger chỉ bật ở Dev (tuỳ ý bật ở Prod nếu cần)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Forwarded headers từ Fly edge
            var fwd = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fwd.KnownNetworks.Clear();
            fwd.KnownProxies.Clear();
            app.UseForwardedHeaders(fwd);

            // KHÔNG redirect HTTPS ở Prod (Fly đã terminate TLS).
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseCors("FE");

            // Map cookie -> Authorization Bearer
            app.UseMiddleware<CookieJwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
