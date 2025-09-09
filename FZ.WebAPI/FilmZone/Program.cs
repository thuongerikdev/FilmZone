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

            // Ưu tiên ENV (Fly secrets)
            builder.Configuration.AddEnvironmentVariables();

            // .env chỉ cho DEV
            if (builder.Environment.IsDevelopment())
            {
                var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
                if (File.Exists(envPath)) DotNetEnv.Env.Load(envPath);
            }

            // Kestrel: Prod (Fly) chỉ HTTP:8080, Dev muốn gì giữ ở appsettings.Development.json
            builder.WebHost.ConfigureKestrel((ctx, opt) =>
            {
                if (ctx.HostingEnvironment.IsDevelopment())
                {
                    // dev có thể cấu hình trong appsettings.Development.json nếu thích
                    opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                }
                else
                {
                    opt.ListenAnyIP(8080); // PROD: HTTP only
                }
            });

            // Services
            builder.ConfigureAuth(typeof(Program).Namespace);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddStackExchangeRedisCache(o =>
            {
                // Redis__ConnectionString (secret) → "Redis:ConnectionString"
                o.Configuration = builder.Configuration["Redis:ConnectionString"];
                o.InstanceName = "FilmZone";
            });

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
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // ✅ Luôn bật Swagger ở mọi môi trường
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FilmZone API v1");
                c.RoutePrefix = "swagger"; // UI tại /swagger
            });

            // Dev mới dùng HTTPS redirect (Prod Fly chạy HTTP:8080)
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Forwarded headers từ Fly edge
            var fwd = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fwd.KnownNetworks.Clear();
            fwd.KnownProxies.Clear();
            app.UseForwardedHeaders(fwd);

            app.UseRouting();
            app.UseCors("FE");
            app.UseMiddleware<CookieJwtMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
