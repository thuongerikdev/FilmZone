using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.ApplicationService.MFAService.Implements;
using FZ.Auth.ApplicationService.MFAService.Implements.User;
using FZ.Auth.Infrastructure;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Implements;
using FZ.Constant;
using FZ.Constant.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using FZ.Auth.ApplicationService.MFAService.Implements.Role;
using FZ.Auth.ApplicationService.MFAService.Implements.Account;

namespace FZ.Auth.ApplicationService.StartUp
{
    public static class AuthStartUp
    {
        /// <summary>
        /// Register all Auth services, DB, Redis, AuthN/Z, CORS.
        /// Call this in Program.cs BEFORE Build().
        /// </summary>
        public static void ConfigureAuth(this WebApplicationBuilder builder, string? assemblyName)
        {
           

            // ✅ Trong ConfigureAuth:
            builder.Services.AddDbContext<AuthDbContext>(
               options =>
               {
                   options.UseSqlServer(
                       builder.Configuration.GetConnectionString("Default"),
                       sqlOptions =>
                       {
                           sqlOptions.MigrationsAssembly(assemblyName);
                           sqlOptions.MigrationsHistoryTable(DbSchema.TableMigrationsHistory, DbSchema.Auth);
                       });
               },
               ServiceLifetime.Scoped
           );


            // === Redis ===
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(configuration);
            });


            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("MailSettings"));



            // === Common infrastructure ===
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IDeviceIdProvider, DeviceIdProvider>();

            // === Repositories ===
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenGenerate, TokenGenerate>();
            builder.Services.AddScoped<IEmailTokenRepository, TokenRepository>();
            builder.Services.AddScoped<IMFARepository, MFARepository>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            builder.Services.AddScoped<IAuthUserSessionRepository, AuthUserSessionRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
            builder.Services.AddScoped<IResetTicketStore, ResetTicketStore>();



            // === UoW & Services ===
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthRegisterService, AuthRegisterService>();
            builder.Services.AddScoped<IAuthLoginService, AuthLoginService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthRoleService, AuthRoleService>();
            builder.Services.AddScoped<IAuthUserService, AuthUserService>();
            builder.Services.AddScoped<IPasswordChangeService, PasswordChangeService>();
            builder.Services.AddScoped<IMfaService, MfaService>();




            // === Authentication (JWT bearer as default) ===
            var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "A_very_long_and_secure_secret_key_1234567890";
            var key = Encoding.UTF8.GetBytes(secretKey);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true; // set false only for local dev
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            if (ctx.Exception is SecurityTokenExpiredException)
                                ctx.Response.Headers["x-token-expired"] = "true";
                            return Task.CompletedTask;
                        },
                        OnChallenge = ctx =>
                        {
                            if (!ctx.Response.HasStarted)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                ctx.Response.Headers.TryAdd("WWW-Authenticate", "Bearer error=\"invalid_token\", error_description=\"The access token is expired\"");
                            }
                            ctx.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                })
                // External cookie for OAuth handshakes
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
                {
                    opt.Cookie.Name = "external.auth";
                    opt.Cookie.SameSite = SameSiteMode.None;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                })
                // Google OAuth (optional)
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.CallbackPath = "/signin-google";
                    options.SaveTokens = true;

                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                    // Backchannel with custom handler (IPv4-only, TLS 1.2/1.3)
                    var handler = new SocketsHttpHandler
                    {
                        UseProxy = false,
                        AutomaticDecompression = DecompressionMethods.All,
                        ConnectTimeout = TimeSpan.FromSeconds(8),
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                        {
                            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                        },
                        ConnectCallback = async (ctx, ct) =>
                        {
                            var addresses = await Dns.GetHostAddressesAsync(ctx.DnsEndPoint!.Host, ct);
                            var ipv4 = Array.Find(addresses, ip => ip.AddressFamily == AddressFamily.InterNetwork);
                            if (ipv4 == null) throw new SocketException((int)SocketError.AddressNotAvailable);

                            var ep = new IPEndPoint(ipv4, ctx.DnsEndPoint.Port);
                            var s = new Socket(ipv4.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            s.NoDelay = true;
                            using var reg = ct.Register(() => { try { s.Dispose(); } catch { } });
                            await s.ConnectAsync(ep, ct);
                            return new NetworkStream(s, ownsSocket: true);
                        }
                    };

                    options.Backchannel = new HttpClient(handler)
                    {
                        Timeout = TimeSpan.FromSeconds(20),
                        DefaultRequestVersion = HttpVersion.Version11,
#if NET8_0_OR_GREATER
                        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
#endif
                    };
                });

            // === Authorization (permissions) ===
            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in PermissionConstants.Permissions)
                {
                    options.AddPolicy(permission.Key, policy => policy.RequireClaim("permission", permission.Value));
                }
            });

            // === CORS for FE domains ===
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("FE", p =>
                {
                    if (allowedOrigins.Length > 0)
                        p.WithOrigins(allowedOrigins);
                    else
                        p.WithOrigins("http://localhost:3000"); // dev fallback

                    p.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
                });
            });

            // === Forwarded headers (behind proxy/load balancer) ===
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Optionally trust specific proxies:
                // opts.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            });
        }

        /// <summary>
        /// Configure middleware pipeline. Call AFTER Build().
        /// </summary>
      
    }
}

// ================= Program.cs sample =================
// using FZ.Auth.ApplicationService.StartUp;
// var builder = WebApplication.CreateBuilder(args);
// builder.ConfigureAuth(assemblyName: typeof(Program).Assembly.FullName);
// var app = builder.Build();
// app.ConfigureAuthPipeline();
// app.Run();
