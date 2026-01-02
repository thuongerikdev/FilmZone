using FZ.Auth.Domain.MFA;                // AuthUserSession
using FZ.Auth.Domain.Token;               // AuthRefreshToken
using FZ.Auth.Domain.User;                // AuthUser
using FZ.Auth.Infrastructure.Repository.Abtracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Implements
{
    public sealed class TokenGenerate : ITokenGenerate
    {
        private readonly AuthDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDb;
        private readonly IHttpContextAccessor _http;

        public TokenGenerate(
            IConnectionMultiplexer redis,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            AuthDbContext db)
        {
            _db = db;
            _configuration = configuration;
            _redisDb = redis.GetDatabase();
            _http = httpContextAccessor;
        }

        // ===== Helpers =====
        private static string StripPort(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return ip;
            ip = ip.Trim();

            if (ip.StartsWith("[")) // [IPv6]:port
            {
                var end = ip.IndexOf(']');
                return end >= 0 ? ip.Substring(1, end - 1) : ip;
            }

            var firstColon = ip.IndexOf(':');
            var lastColon = ip.LastIndexOf(':');
            if (firstColon == lastColon && firstColon > -1) // IPv4:port
            {
                var portPart = ip.Substring(lastColon + 1);
                if (int.TryParse(portPart, out _))
                    return ip.Substring(0, lastColon);
            }
            return ip; // IPv6 or no port
        }

        public string? GetClientIp()
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            var ip = ctx.Connection.RemoteIpAddress;
            if (ip is not null)
            {
                if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
                return ip.ToString();
            }

            // fallback parse common proxy headers
            var h = ctx.Request.Headers;
            var candidates = new[] { "CF-Connecting-IP", "True-Client-IP", "X-Real-IP", "X-Forwarded-For" };
            foreach (var key in candidates)
            {
                var raw = h[key].ToString();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var first = raw.Split(',')[0].Trim();
                first = StripPort(first);
                if (IPAddress.TryParse(first, out var addr))
                {
                    if (addr.IsIPv4MappedToIPv6) addr = addr.MapToIPv4();
                    return addr.ToString();
                }
            }
            return null;
        }

        public string? GetUserAgent() => _http.HttpContext?.Request.Headers[HeaderNames.UserAgent].ToString();

        public Task AddTokenAsync(AuthRefreshToken token, CancellationToken ct)
            => _db.authRefreshTokens.AddAsync(token, ct).AsTask();

        private string SecretKey => _configuration["Jwt:SecretKey"] ?? "A_very_long_and_secure_secret_key_1234567890";

        private static string CacheKeyAccess(int userId, int sessionId) => $"user:{userId}:sess:{sessionId}:access";
        private static string CacheKeyRefresh(int userId, int sessionId) => $"user:{userId}:sess:{sessionId}:refresh";

        // ================= 1. Issue Pair (Login logic cũ - tự query quyền) =================
        public async Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user,
            int sessionId,
            string? createdByIp,
            TimeSpan? accessTtl = null,
            TimeSpan? refreshTtl = null)
        {
            // Không truyền permissions -> overridePermissions = null -> Tự query DB
            var access = await CreateAccessTokenFromUserAsync(user, accessTtl ?? TimeSpan.FromMinutes(1), sessionId, null);

            var refresh = await GenerateUniqueRefreshTokenAsync(
                userId: user.userID,
                sessionId: sessionId,
                ip: createdByIp,
                ttl: refreshTtl ?? TimeSpan.FromDays(7));

            _db.authRefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            await _redisDb.StringSetAsync(CacheKeyAccess(user.userID, sessionId), access, TimeSpan.FromHours(1));
            await _redisDb.StringSetAsync(CacheKeyRefresh(user.userID, sessionId), refresh.Token, TimeSpan.FromDays(7));

            return (access, refresh);
        }

        // ================= 2. NEW: Issue Pair (Login logic mới - Nhận Permissions từ bên ngoài) =================
        public async Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user,
            int sessionId,
            string createdByIp,
            TimeSpan accessTtl,
            TimeSpan refreshTtl,
            List<string> permissions) // <--- NHẬN LIST PERMISSION
        {
            // Truyền permissions vào hàm tạo token để tránh query DB
            var access = await CreateAccessTokenFromUserAsync(user, accessTtl, sessionId, permissions);

            var refresh = await GenerateUniqueRefreshTokenAsync(
                userId: user.userID,
                sessionId: sessionId,
                ip: createdByIp,
                ttl: refreshTtl);

            _db.authRefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            // Cache
            await _redisDb.StringSetAsync(CacheKeyAccess(user.userID, sessionId), access, TimeSpan.FromHours(1));
            await _redisDb.StringSetAsync(CacheKeyRefresh(user.userID, sessionId), refresh.Token, TimeSpan.FromDays(7));

            return (access, refresh);
        }

        // ================= 3. Legacy IssuePair (No SessionId) =================
        public async Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user,
            string? createdByIp,
            TimeSpan? accessTtl = null,
            TimeSpan? refreshTtl = null)
        {
            var ua = GetUserAgent() ?? string.Empty;
            var ip = createdByIp ?? GetClientIp() ?? string.Empty;

            var session = new AuthUserSession
            {
                userID = user.userID,
                deviceId = "legacy",
                ip = ip,
                userAgent = ua,
                createdAt = DateTime.UtcNow,
                lastSeenAt = DateTime.UtcNow,
                isRevoked = false
            };
            _db.authUserSessions.Add(session);
            await _db.SaveChangesAsync();

            // Gọi overload cũ (tự query quyền)
            return await IssuePairAsync(user, session.sessionID, createdByIp, accessTtl, refreshTtl);
        }

        // ================= 4. Rotate (Refresh Token) =================
        public async Task<(string accessToken, AuthRefreshToken newRefresh)> RotateAsync(
            string incomingRefreshToken,
            string? ip,
            TimeSpan? accessTtl = null,
            TimeSpan? refreshTtl = null)
        {
            var current = await _db.authRefreshTokens
                .Include(r => r.user)
                .FirstOrDefaultAsync(r => r.Token == incomingRefreshToken);

            if (current == null)
                throw new UnauthorizedAccessException("Invalid refresh token.");
            if (!current.IsActive)
                throw new UnauthorizedAccessException("Refresh token is not active.");

            // revoke current
            current.Revoked = DateTime.UtcNow;
            current.RevokedByIp = ip;

            // new RT keeps same sessionId
            var newRefresh = await GenerateUniqueRefreshTokenAsync(
                userId: current.userID,
                sessionId: current.sessionID,
                ip: ip,
                ttl: refreshTtl ?? TimeSpan.FromDays(7));

            current.ReplacedByToken = newRefresh.Token;
            _db.authRefreshTokens.Add(newRefresh);

            // Khi Rotate, client không gửi lại list permission -> Truyền null để tự query DB lấy quyền mới nhất
            var access = await CreateAccessTokenFromUserAsync(current.user, accessTtl ?? TimeSpan.FromMinutes(30), current.sessionID, null);

            await _db.SaveChangesAsync();

            await _redisDb.StringSetAsync(CacheKeyAccess(current.userID, current.sessionID), access, TimeSpan.FromHours(1));
            await _redisDb.StringSetAsync(CacheKeyRefresh(current.userID, current.sessionID), newRefresh.Token, TimeSpan.FromDays(7));

            return (access, newRefresh);
        }

        // ================= Revoke Methods =================
        public async Task RevokeAsync(string refreshToken, string? ip)
        {
            var token = await _db.authRefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (token == null || !token.IsActive) return;

            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ip;
            await _db.SaveChangesAsync();

            await _redisDb.KeyDeleteAsync(CacheKeyAccess(token.userID, token.sessionID));
            await _redisDb.KeyDeleteAsync(CacheKeyRefresh(token.userID, token.sessionID));
        }

        public async Task<int> RevokeBySessionAsync(int userId, int sessionId, string? ip)
        {
            var now = DateTime.UtcNow;
            var tokens = await _db.authRefreshTokens
                .Where(t => t.userID == userId
                         && t.sessionID == sessionId
                         && t.Revoked == null
                         && t.Expires > now)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.Revoked = now;
                t.RevokedByIp = ip;
            }

            if (tokens.Count > 0)
                await _db.SaveChangesAsync();

            await _redisDb.KeyDeleteAsync(CacheKeyAccess(userId, sessionId));
            await _redisDb.KeyDeleteAsync(CacheKeyRefresh(userId, sessionId));

            return tokens.Count;
        }

        public async Task<int> RevokeAllForUserAsync(int userId, string? ip)
        {
            var now = DateTime.UtcNow;

            var tokens = await _db.authRefreshTokens
                .Where(t => t.userID == userId && t.Revoked == null && t.Expires > now)
                .ToListAsync();

            var sessions = tokens.Select(t => t.sessionID).Distinct().ToList();

            foreach (var t in tokens)
            {
                t.Revoked = now;
                t.RevokedByIp = ip;
            }

            if (tokens.Count > 0)
                await _db.SaveChangesAsync();

            foreach (var sid in sessions)
            {
                await _redisDb.KeyDeleteAsync(CacheKeyAccess(userId, sid));
                await _redisDb.KeyDeleteAsync(CacheKeyRefresh(userId, sid));
            }

            return tokens.Count;
        }

        public async Task<(AuthRefreshToken token, AuthUser user)?> GetActiveAsync(string refreshToken)
        {
            var token = await _db.authRefreshTokens
                .Include(r => r.user)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (token == null || !token.IsActive) return null;
            return (token, token.user);
        }

        public async Task<AuthRefreshToken> GenerateUniqueRefreshTokenAsync(int userId, int sessionId, string? ip, TimeSpan ttl)
        {
            string token;
            do
            {
                using var rng = RandomNumberGenerator.Create();
                var bytes = new byte[64];
                rng.GetBytes(bytes);
                token = Convert.ToBase64String(bytes);
            } while (await _db.authRefreshTokens.AnyAsync(r => r.Token == token));

            return new AuthRefreshToken
            {
                userID = userId,
                sessionID = sessionId,
                Token = token,
                Expires = DateTime.UtcNow.Add(ttl),
                Created = DateTime.UtcNow,
                CreatedByIp = ip
            };
        }

        // ================= PRIVATE GENERATE TOKEN (Refactored) =================
        private async Task<string> CreateAccessTokenFromUserAsync(
            AuthUser user,
            TimeSpan? ttl = null,
            int? sessionId = null,
            List<string>? overridePermissions = null)
        {
            // 1. Roles
            var roleIds = await _db.authUserRoles
                .Where(x => x.userID == user.userID)
                .Select(x => new { x.roleID, x.role.roleName })
                .ToListAsync();

            // 2. Permissions Logic (Updated for Entity.Action.Scope)
            var finalPermissions = new HashSet<string>();

            if (overridePermissions != null)
            {
                foreach (var p in overridePermissions) finalPermissions.Add(p);
            }
            else
            {
                // CASE B: Refresh Token -> Query lại DB
                var dbPerms = await (from ur in _db.authUserRoles
                                     join rp in _db.authRolePermissions on ur.roleID equals rp.roleID
                                     join p in _db.authPermissions on rp.permissionID equals p.permissionID
                                     where ur.userID == user.userID && !string.IsNullOrEmpty(p.code)
                                     // 👇 SỬA Ở ĐÂY: Chỉ lấy Code
                                     select p.code)
                                     .ToListAsync();

                foreach (var p in dbPerms) finalPermissions.Add(p);
            }

            // 3. User Info & Claims
            var profile = await _db.authProfiles.FirstOrDefaultAsync(x => x.userID == user.userID);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("name", profile?.lastName ?? string.Empty),
                new Claim("email", user.email),
                new Claim("userName", user.userName),
                new Claim("userId", user.userID.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.userID.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (sessionId.HasValue)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sid, sessionId.Value.ToString()));

            foreach (var r in roleIds) claims.Add(new Claim("role", r.roleName));

            // Claim Permission bây giờ sẽ có dạng "Movie.get.admin"
            foreach (var p in finalPermissions) claims.Add(new Claim("permission", p));

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(15)),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
