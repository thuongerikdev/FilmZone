using FZ.Auth.Domain.MFA;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Implements
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AuthDbContext _db;
        public AuditLogRepository(AuthDbContext db) => _db = db;
        public async Task LogAsync(Domain.MFA.AuthAuditLog authAuditLog, CancellationToken ct)
        {
            await _db.authAuditLogs.AddAsync(authAuditLog, ct);
        }
    }

    //public sealed class MFARepository : IMFARepository
    //{
    //    private readonly AuthDbContext _db;
    //    public MFARepository(AuthDbContext db) => _db = db;

    //    public Task<bool> CheckEnabledMFAAsync(int userId, CancellationToken ct)
    //    {
    //        return _db.authMfaSecrets.AnyAsync(x => x.userID == userId && x.isEnabled == true, ct);
    //    }
    //    public Task AddMFAAsync(Domain.MFA.AuthMfaSecret authMFA, CancellationToken ct)
    //        => _db.authMfaSecrets.AddAsync(authMFA, ct).AsTask();

    //    public Task<Domain.MFA.AuthMfaSecret?> GetByUserIdAsync(int userId, CancellationToken ct)
    //        => _db.authMfaSecrets.FirstOrDefaultAsync(x => x.userID == userId, ct);
    //    //public Task UpdateMFAAsync(Domain.MFA.AuthMfaSecret authMFA, CancellationToken ct)
    //    //{
    //    //    _db.authMfaSecrets.Update(authMFA); // hoặc để EF tracking tự detect
    //    //    return Task.CompletedTask;
    //    //}
    //}
    public sealed class MFARepository : IMFARepository
    {
        private readonly AuthDbContext _db;
        public MFARepository(AuthDbContext db) => _db = db;

        public Task<AuthMfaSecret?> GetByUserAsync(int userId, CancellationToken ct)
            => _db.authMfaSecrets.FirstOrDefaultAsync(x => x.userID == userId && x.type == "TOTP", ct);

        public async Task UpsertAsync(AuthMfaSecret entity, CancellationToken ct)
        {
            var tracked = await _db.authMfaSecrets
                .FirstOrDefaultAsync(x => x.userID == entity.userID && x.type == entity.type, ct);

            if (tracked == null)
                await _db.authMfaSecrets.AddAsync(entity, ct);
            else
            {
                _db.Entry(tracked).CurrentValues.SetValues(entity);
                _db.authMfaSecrets.Update(tracked);
            }
        }

        public Task<bool> CheckEnabledMFAAsync(int userId, CancellationToken ct)
            => _db.authMfaSecrets.AnyAsync(x => x.userID == userId && x.type == "TOTP" && x.status == "Enabled", ct);
    }


    public sealed class AuthUserSessionRepository : IAuthUserSessionRepository
    {
        private readonly AuthDbContext _db;
        public AuthUserSessionRepository(AuthDbContext db) => _db = db;

        public Task AddSessionAsync(AuthUserSession session, CancellationToken ct)
            => _db.authUserSessions.AddAsync(session, ct).AsTask();

        public Task<AuthUserSession?> FindByIdAsync(int sessionId, CancellationToken ct)
            => _db.authUserSessions.FirstOrDefaultAsync(s => s.sessionID == sessionId, ct);

        public async Task MarkRevokedAsync(int sessionId, CancellationToken ct)
        {
            var s = await _db.authUserSessions.FirstOrDefaultAsync(x => x.sessionID == sessionId, ct);
            if (s is null) return;
            s.isRevoked = true;
            s.lastSeenAt = DateTime.UtcNow;
            _db.authUserSessions.Update(s);
            // Không SaveChanges ở đây; để UoW gọi
        }

        // NEW
        public Task<int> MarkAllRevokedForUserAsync(int userId, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // Cập nhật hàng loạt trực tiếp trên DB, KHÔNG cần SaveChanges
            return _db.authUserSessions
                .Where(s => s.userID == userId && !s.isRevoked)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(s => s.isRevoked, true)
                    .SetProperty(s => s.lastSeenAt, now),
                    ct);
        }
    }


    public sealed class DeviceIdProvider : IDeviceIdProvider
    {
        private readonly IHttpContextAccessor _http;
        private const string CookieName = "fz.did";
        private static readonly Regex Safe = new("^[A-Za-z0-9_-]{16,128}$", RegexOptions.Compiled);

        public DeviceIdProvider(IHttpContextAccessor http) => _http = http;

        public string GetOrCreate()
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return Guid.NewGuid().ToString("N");

            var did = ctx.Request.Cookies[CookieName];
            if (string.IsNullOrWhiteSpace(did) || !Safe.IsMatch(did))
            {
                did = Guid.NewGuid().ToString("N"); // ⬅️ dùng GUID thay ULID

                ctx.Response.Cookies.Append(CookieName, did, new CookieOptions
                {
                    SameSite = SameSiteMode.Lax, // khác domain thì dùng None
                    Secure = true,
                    HttpOnly = false,
                    Expires = DateTimeOffset.UtcNow.AddYears(2),
                    IsEssential = true,
                    Path = "/"
                });
            }
            return did;
        }
    }

}
