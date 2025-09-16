using FZ.Auth.ApplicationService.Billing;
using FZ.Auth.ApplicationService.Billing.PaymentModule;
using FZ.Auth.ApplicationService.Common;
using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.ApplicationService.MFAService.Implements;
using FZ.Auth.ApplicationService.MFAService.Implements.Account;
using FZ.Auth.ApplicationService.MFAService.Implements.Role;
using FZ.Auth.ApplicationService.MFAService.Implements.User;
using FZ.Auth.Infrastructure;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Billing;
using FZ.Auth.Infrastructure.Repository.Implements;
using FZ.Constant;
using FZ.Constant.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace FZ.Auth.ApplicationService.StartUp
{
    public static class AuthStartUp
    {
        public static void ConfigureAuth(this WebApplicationBuilder builder, string? assemblyName)
        {
            // === DB & other services ===
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

            builder.Services.AddHostedService<SubscriptionExpiryWorker>();
            builder.Services.AddScoped<IAuthorizationHandler, ActiveVipHandler>();

            // === Redis (unchanged from your working version) ===
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Redis");
                var config = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

                // Parse rediss://... or host:port,...
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
                    var tmp = ConfigurationOptions.Parse(config);
                    if (tmp.EndPoints.Count == 1 && tmp.EndPoints[0] is DnsEndPoint dns)
                    {
                        host = dns.Host;
                        port = dns.Port;
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
                    ClientName = "auth-service",
                };

                if (!string.IsNullOrEmpty(host))
                {
                    options.EndPoints.Add(host, port);
                }
                else
                {
                    options = ConfigurationOptions.Parse(config);
                }

                if (isTls)
                {
                    options.Ssl = true;
                    options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
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
                    logger.LogError(ex, "Redis Connect failed (config prefix: {prefix})", (config ?? "").Split('@').FirstOrDefault());
                    throw;
                }
            });

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("MailSettings"));

            // === Common infra & repositories (unchanged) ===
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IDeviceIdProvider, DeviceIdProvider>();

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
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
            builder.Services.AddScoped<IPlanRepository, PlanRepository>();
            builder.Services.AddScoped<IPriceRepository, PriceRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthRegisterService, AuthRegisterService>();
            builder.Services.AddScoped<IAuthLoginService, AuthLoginService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthRoleService, AuthRoleService>();
            builder.Services.AddScoped<IAuthUserService, AuthUserService>();
            builder.Services.AddScoped<IPasswordChangeService, PasswordChangeService>();
            builder.Services.AddScoped<IMfaService, MfaService>();

            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

            // === Authentication (JWT + Cookies + External + Google) ===
            var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "A_very_long_and_secure_secret_key_1234567890";
            var key = Encoding.UTF8.GetBytes(secretKey);
            var isDev = builder.Environment.IsDevelopment();

            var googleClientId = builder.Configuration["Google:ClientId"] ?? builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Google:ClientSecret"] ?? builder.Configuration["Authentication:Google:ClientSecret"];
            var googleCallbackPath = builder.Configuration["Google:CallbackPath"] ?? builder.Configuration["Authentication:Google:CallbackPath"] ?? "/signin-google";

            builder.Services
                .AddAuthentication(options =>
                {
                    // keep JWT as default authenticate/challenge (API semantic)
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                    // use cookie as default sign-in scheme (so SignInAsync uses cookie by default)
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = !isDev;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),

                        RoleClaimType = "role",
                        NameClaimType = "userName"
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
                                ctx.Response.Headers.TryAdd("WWW-Authenticate",
                                    "Bearer error=\"invalid_token\", error_description=\"The access token is expired\"");
                            }
                            ctx.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                })
                // Register the app session cookie ("Cookies")
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "fz.auth";
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    // optional: paths
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/access-denied";
                    // sliding expiration, expiry, etc. adjust if needed
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                })
                // External cookie for OAuth handshake (temporary)
                .AddCookie("External", opt =>
                {
                    opt.Cookie.Name = "external.auth";
                    opt.Cookie.SameSite = SameSiteMode.None;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            // Only add Google if client id/secret present
            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                builder.Services.AddAuthentication() // continue adding handlers to the same auth builder
                    .AddGoogle("Google", options =>
                    {
                        options.ClientId = googleClientId!;
                        options.ClientSecret = googleClientSecret!;
                        // SignInScheme should be the temporary external cookie
                        options.SignInScheme = "External";
                        options.CallbackPath = googleCallbackPath;
                        options.SaveTokens = true;

                        options.CorrelationCookie.SameSite = SameSiteMode.None;
                        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                        // Backchannel settings (as you had)
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
            }

            // === Authorization & other policies ===
            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in PermissionConstants.Permissions)
                {
                    options.AddPolicy(permission.Key, policy => policy.RequireClaim("permission", permission.Value));
                }
                options.AddPolicy("ActiveVIP", p => p.Requirements.Add(new ActiveVipRequirement()));
            });

            // === CORS ===
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("FE", p =>
                {
                    if (allowedOrigins.Length > 0)
                        p.WithOrigins(allowedOrigins);
                    else
                        p.WithOrigins("http://localhost:3000");

                    p.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
                });
            });

            // === Forwarded headers (behind proxy) ===
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

    }
}
