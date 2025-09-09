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

            //new DotNetEnv.EnvLoader().Load();
            builder.Configuration.AddEnvironmentVariables(); // mặc định đã có



            try
            {
                // chỉ load nếu file tồn tại
                var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
                if (File.Exists(envPath))
                    DotNetEnv.Env.Load(envPath);
            }
            catch { /* ignore */ }

            // Modules
            builder.ConfigureAuth(typeof(Program).Namespace);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Redis cache (tuỳ dùng)
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["Redis:ConnectionString"]; // ✅
                options.InstanceName = "SampleInstance";
            });

            // Swagger
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
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
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
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
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();


            app.UseRouting();

            // CORS cho FE (gửi cookie)
            app.UseCors("FE");

            // Map cookie -> Authorization header Bearer (phải trước Authenticate)
            app.UseMiddleware<CookieJwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();



            app.Run();
        }
    }
}