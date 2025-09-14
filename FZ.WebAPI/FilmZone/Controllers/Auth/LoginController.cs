using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Dtos.User;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FZ.WebAPI.Controllers.Auth
{
    [Route("login")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IAuthLoginService _authLoginService;
        private readonly ITokenGenerate _tokenGenerate;
        private readonly IConfiguration _cfg;
        public LoginController(IAuthLoginService authLoginService , ITokenGenerate tokenGenerate , IConfiguration cfg)
        {
            _authLoginService = authLoginService;
            _tokenGenerate = tokenGenerate;
            _cfg = cfg;
        }
        [HttpPost("userLogin")]
        public async Task<IActionResult> UserLogin( [FromBody] LoginRequest loginRequest , CancellationToken ct)
        {
            var result = await _authLoginService.LoginAsync(loginRequest, ct);
            if (result.ErrorCode == 200)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), "Login", new { returnUrl = safeReturnUrl })
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromServices] IAuthLoginService svc, CancellationToken ct)
        {
            var rt = Request.Cookies["fz.refresh"]; // nếu bạn lưu RT vào HttpOnly cookie
            if (string.IsNullOrWhiteSpace(rt))
                return Ok(ResponseConst.Success("Đã đăng xuất.", true));

            var res = await svc.LogoutByRefreshTokenAsync(rt, ct);

            // Xoá cookie
            Response.Cookies.Delete("fz.refresh", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });

            return Ok(res);
        }

        [HttpPost("logout/session/{sessionId:int}")]
        public async Task<IActionResult> LogoutSession(int sessionId, [FromServices] IAuthLoginService svc, CancellationToken ct)
        {
            // lấy userId từ JWT (claim "userId")
            var userIdStr = User.FindFirstValue("userId");
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var res = await svc.LogoutSessionAsync(userId, sessionId, ct);
            return Ok(res);
        }

        [HttpPost("logout/all")]
        public async Task<IActionResult> LogoutAll([FromServices] IAuthLoginService svc, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue("userId");
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var res = await svc.LogoutAllDevicesAsync(userId, ct);
            // xóa cookie RT hiện tại (nếu dùng cookie)
            Response.Cookies.Delete("fz.refresh", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });

            return Ok(res);
        }

        [HttpGet("google/callback")]
        [HttpGet("/signin-google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl, CancellationToken ct)
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);

            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authResult?.Succeeded != true || authResult.Principal is null)
            {
                return Redirect(AppendQuery(safeReturnUrl, "error", "google_auth_failed"));
            }

            var p = authResult.Principal;
            string? sub = p.FindFirstValue(ClaimTypes.NameIdentifier) ?? p.FindFirstValue("sub");
            string? email = p.FindFirstValue(ClaimTypes.Email);
            string? fullName = p.FindFirstValue(ClaimTypes.Name);
            string? given = p.FindFirstValue(ClaimTypes.GivenName);
            string? family = p.FindFirstValue(ClaimTypes.Surname);
            string? picture = p.FindFirstValue("urn:google:picture") ?? p.FindFirstValue("picture");

            if (string.IsNullOrWhiteSpace(fullName))
                fullName = string.Join(' ', new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));

            if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Redirect(AppendQuery(safeReturnUrl, "error", "missing_google_claims"));
            }

            var dto = new AuthLoginGoogleRequest
            {
                GoogleSub = sub,
                email = email,
                fullName = fullName,
                avatar = picture
            };

            var result = await _authLoginService.LoginWithGoogleAsync(dto, ct);

            // Luôn dọn external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.ErrorCode != 200 || result.Data is null)
            {
                var loginUrl = _cfg["Frontend:LoginUrl"] ??
                               $"{_cfg["Frontend:AppUrl"] ?? "http://localhost:3000"}/login";
                return Redirect(AppendQuery(loginUrl, "error", "google_login_failed"));
            }

            var data = result.Data;

            //(data.requiresMfa && !string.IsNullOrWhiteSpace(data.mfaTicket))
            // >>> Trường hợp cần MFA: không set cookie, redirect về FE để nhập TOTP
            if (data.requiresMfa == true && !string.IsNullOrWhiteSpace(data.mfaTicket))
            {
                // ví dụ lấy từ cấu hình
                var appUrl = _cfg["Frontend:AppUrl"] ?? "http://localhost:3000";
                var mfaPath = _cfg["Frontend:MfaVerifyPath"] ?? "/mfa/verify";
                var mfaUrl = $"{appUrl.TrimEnd('/')}{mfaPath}";

                return Redirect(AppendQuery(mfaUrl, "ticket", data.mfaTicket));
            }

            // Đã login thành công (không cần MFA) → set refresh cookie
            if (!string.IsNullOrWhiteSpace(data.refreshToken) && data.refreshTokenExpiration != default)
            {
                Response.Cookies.Append("fz.refresh", data.refreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = data.refreshTokenExpiration
                });
            }

            var appBase = _cfg["Frontend:AppUrl"] ?? "http://localhost:3000";
            var successUrl = $"{appBase.TrimEnd('/')}/business/mainScreen";
            return Redirect(successUrl);

        }

        [HttpPost("mfa/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyMfa([FromBody] MfaLoginVerifyRequest req, CancellationToken ct)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.mfaTicket) || string.IsNullOrWhiteSpace(req.code))
                return BadRequest(ResponseConst.Error<bool>(400, "Thiếu mfaTicket hoặc code"));

            var result = await _authLoginService.VerifyMfaAndLoginAsync(req, ct);
            if (result.ErrorCode != 200 || result.Data is null)
                return BadRequest(result);

            // Set refresh token bằng HttpOnly cookie (để FE gọi /login/auth/refresh lấy access token)
            var data = result.Data;
            if (!string.IsNullOrWhiteSpace(data.refreshToken) && data.refreshTokenExpiration != default)
            {
                Response.Cookies.Append("fz.refresh", data.refreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,   // nếu FE khác domain; cùng domain có thể dùng Lax
                    Path = "/",
                    Expires = data.refreshTokenExpiration
                });
            }

            // Trả về payload (nếu bạn không muốn lộ access token ở body thì giữ nguyên như hiện tại)
            return Ok(result);
        }



        [HttpPost("auth/refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var rtRaw = Request.Cookies["fz.refresh"];
            if (string.IsNullOrEmpty(rtRaw)) return Unauthorized();

            var (access, newRt) = await _tokenGenerate.RotateAsync(
                incomingRefreshToken: rtRaw,
                ip: _tokenGenerate.GetClientIp(),
                accessTtl: TimeSpan.FromMinutes(30),
                refreshTtl: TimeSpan.FromDays(7)
            );

            Response.Cookies.Append("fz.refresh", newRt.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/login/auth/refresh",
                Expires = newRt.Expires
            });


            return Ok(new { token = access, expiresIn = 1800 });
        }



        private string GetSafeReturnUrl(string? returnUrl)
        {
            var allowed = _cfg.GetSection("Frontend:AllowedReturnUrls").Get<string[]>() ?? Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                allowed.Any(a => returnUrl.StartsWith(a, StringComparison.OrdinalIgnoreCase)))
                return returnUrl;

            return _cfg["Frontend:AppUrl"] ?? "http://localhost:3000";
        }

        private static string AppendQuery(string url, string key, string value)
        {
            var sep = url.Contains('?') ? "&" : "?";
            return $"{url}{sep}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
        }

    }
}
