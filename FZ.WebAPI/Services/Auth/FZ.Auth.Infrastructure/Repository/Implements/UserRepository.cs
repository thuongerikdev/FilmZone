using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using Microsoft.EntityFrameworkCore;
using static FZ.Auth.Dtos.User.ProfileResponseDto;

namespace FZ.Auth.Infrastructure.Repository.Implements
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;
        public UserRepository(AuthDbContext db) => _db = db;


        public Task<AuthUser?> Login(string username, string password, CancellationToken ct)
            => _db.authUsers.FirstOrDefaultAsync(x => (x.userName == username || x.email == username) && x.passwordHash == password, ct);

        public async Task<List<string>> GetPermissionsByUserIdAsync(int userId, CancellationToken ct)
        {
            var query = from ur in _db.Set<AuthUserRole>()
                        join rp in _db.Set<AuthRolePermission>() on ur.roleID equals rp.roleID
                        join p in _db.Set<AuthPermission>() on rp.permissionID equals p.permissionID
                        where ur.userID == userId
                        && !string.IsNullOrEmpty(p.code)
                        // 👇 SỬA Ở ĐÂY: Chỉ lấy Code nguyên bản
                        select p.code;

            return await query.Distinct().ToListAsync(ct);
        }

        public Task UpdateUserName(string newUserName, int userId, CancellationToken ct)
        {
            var user = _db.authUsers.FirstOrDefault(x => x.userID == userId);
            if (user != null)
            {
                user.userName = newUserName;
                _db.authUsers.Update(user);
            }
            return Task.CompletedTask;
        }   

        // >>> NEW: trả DTO để không loop
        public Task<List<UserSlimDto>> GetAllSlimAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return _db.authUsers
                .AsNoTracking()
                .Select(u => new UserSlimDto(
                    u.userID,
                    u.userName,
                    u.email,
                    u.status,
                    u.isEmailVerified,

                    u.profile == null
                        ? null
                        : new ProfileDto(
                            u.profile.firstName,
                            u.profile.lastName,
                            u.profile.avatar,
                            u.profile.gender,
                            u.profile.dateOfBirth
                          ),

                    u.mfaSecret == null
                        ? null
                        : new MfaSecretDto(
                            u.mfaSecret.type,
                            u.mfaSecret.isEnabled,
                            u.mfaSecret.updatedAt
                          ),

                    u.sessions
                        .OrderByDescending(s => s.createdAt)
                        .Take(5)
                        .Select(s => new SessionDto(
                            s.sessionID,
                            s.deviceId,
                            s.ip,
                            s.userAgent,
                            s.createdAt,
                            s.lastSeenAt,
                            s.isRevoked
                        ))
                        .ToList(),

                    u.auditLogs
                        .OrderByDescending(a => a.createdAt)
                        .Take(50)
                        .Select(a => new AuditLogDto(
                            a.auditID,
                            a.action,
                            a.result,
                            a.ip,
                            a.userAgent,
                            a.createdAt,
                            a.detail
                        ))
                        .ToList(),

                    u.emailVerifications
                      
                        .OrderByDescending(ev => ev.createdAt)
                        .Select(ev => new EmailVerificationDto(
                            ev.emailVerificationID,
                            ev.createdAt,
                            ev.expiresAt,
                            ev.consumedAt
                        ))
                        .ToList(),

                    u.passwordResets
                        .OrderByDescending(pr => pr.createdAt)
                        .Take(3)
                        .Select(pr => new PasswordResetDto(
                            pr.passwordResetID,
                            pr.createdAt,
                            pr.expiresAt,
                            pr.consumedAt
                        ))
                        .ToList(),

                    u.refreshTokens
                        .OrderByDescending(rt => rt.Created)
                        .Take(20)
                        .Select(rt => new RefreshTokenDto(
                            rt.refreshTokenID,
                            rt.sessionID,
                            rt.Created,
                            rt.Expires,
                            rt.Revoked,
                            rt.ReplacedByToken,
                            rt.Revoked == null && rt.Expires > now
                        ))
                        .ToList(),

                    u.userRoles
                          .Select(ur => new roleDto(
                            ur.role.roleID,
                            ur.role.roleName,
                            ur.role.roleDescription
                        ))
                        .Distinct()
                        .ToList(),

                   u.userRoles
                        .SelectMany(ur => ur.role.rolePermissions)
                        .Select(rp => new permissionDto(
                            rp.permission.permissionID,
                            rp.permission.permissionName,
                            rp.permission.code
                        ))
                        .Distinct() // Quan trọng: 1 user có nhiều role, các role có thể trùng permission
                        .ToList()
                ))
                .ToListAsync(ct);
        }
        public async Task<UserSlimDto?> GetSlimUserByID ( int userID, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return await _db.authUsers
                .AsNoTracking()
                .AsSplitQuery() // <--- QUAN TRỌNG: Ngăn chặn bùng nổ dữ liệu (Cartesian Explosion)
                .Where(x => x.userID == userID)
                .Select(u => new UserSlimDto(
                    u.userID,
                    u.userName,
                    u.email,
                    u.status,
                    u.isEmailVerified,

                    // --- Profile ---
                    u.profile == null ? null : new ProfileDto(
                        u.profile.firstName,
                        u.profile.lastName,
                        u.profile.avatar,
                        u.profile.gender,
                        u.profile.dateOfBirth
                    ),

                    // --- MFA ---
                    u.mfaSecret == null ? null : new MfaSecretDto(
                        u.mfaSecret.type,
                        u.mfaSecret.isEnabled,
                        u.mfaSecret.updatedAt
                    ),

                    // --- Sessions (Top 5) ---
                    u.sessions
                        .OrderByDescending(s => s.createdAt)
                        .Take(5)
                        .Select(s => new SessionDto(
                            s.sessionID,
                            s.deviceId,
                            s.ip,
                            s.userAgent,
                            s.createdAt,
                            s.lastSeenAt,
                            s.isRevoked
                        ))
                        .ToList(),

                    // --- Audit Logs (Top 50) ---
                    u.auditLogs
                        .OrderByDescending(a => a.createdAt)
                        .Take(50)
                        .Select(a => new AuditLogDto(
                            a.auditID,
                            a.action,
                            a.result,
                            a.ip,
                            a.userAgent,
                            a.createdAt,
                            a.detail
                        ))
                        .ToList(),

                    // --- Email Verifications ---
                    u.emailVerifications
                        .OrderByDescending(ev => ev.createdAt)
                        .Select(ev => new EmailVerificationDto(
                            ev.emailVerificationID,
                            ev.createdAt,
                            ev.expiresAt,
                            ev.consumedAt
                        ))
                        .ToList(),

                    // --- Password Resets (Top 3) ---
                    u.passwordResets
                        .OrderByDescending(pr => pr.createdAt)
                        .Take(3)
                        .Select(pr => new PasswordResetDto(
                            pr.passwordResetID,
                            pr.createdAt,
                            pr.expiresAt,
                            pr.consumedAt
                        ))
                        .ToList(),

                    // --- Refresh Tokens (Top 20) ---
                    u.refreshTokens
                        .OrderByDescending(rt => rt.Created)
                        .Take(20)
                        .Select(rt => new RefreshTokenDto(
                            rt.refreshTokenID,
                            rt.sessionID,
                            rt.Created,
                            rt.Expires,
                            rt.Revoked,
                            rt.ReplacedByToken,
                            rt.Revoked == null && rt.Expires > now // IsActive logic
                        ))
                        .ToList(),

                    // --- Roles ---
                    u.userRoles
                        .Select(ur => new roleDto(
                            ur.role.roleID,
                            ur.role.roleName,
                            ur.role.roleDescription
                        ))
                        .Distinct() // Tránh trùng lặp nếu data lỗi
                        .ToList(),

                    // --- Permissions ---
                    u.userRoles
                        .SelectMany(ur => ur.role.rolePermissions)
                        .Select(rp => new permissionDto(
                            rp.permission.permissionID,
                            rp.permission.permissionName,
                            rp.permission.code
                        ))
                        .Distinct() // Quan trọng: 1 user có nhiều role, các role có thể trùng permission
                        .ToList()
                ))
                .FirstOrDefaultAsync(ct);
        }
        public async Task<UserSlimDto?> GetSlimUserWhereScopeUserByID(int userID, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return await _db.authUsers
                .AsNoTracking()
                .AsSplitQuery() // Ngăn chặn bùng nổ dữ liệu (Cartesian Explosion)
                .Where(x => x.userID == userID && x.scope == "user")
                .Select(u => new UserSlimDto(
                    u.userID,
                    u.userName,
                    u.email,
                    u.status,
                    u.isEmailVerified,

                    // --- Profile ---
                    u.profile == null ? null : new ProfileDto(
                        u.profile.firstName,
                        u.profile.lastName,
                        u.profile.avatar,
                        u.profile.gender,
                        u.profile.dateOfBirth
                    ),

                    // --- MFA ---
                    u.mfaSecret == null ? null : new MfaSecretDto(
                        u.mfaSecret.type,
                        u.mfaSecret.isEnabled,
                        u.mfaSecret.updatedAt
                    ),

                    // --- Sessions (Top 5) ---
                    u.sessions
                        .OrderByDescending(s => s.createdAt)
                        .Take(5)
                        .Select(s => new SessionDto(
                            s.sessionID,
                            s.deviceId,
                            s.ip,
                            s.userAgent,
                            s.createdAt,
                            s.lastSeenAt,
                            s.isRevoked
                        ))
                        .ToList(),

                    // --- Audit Logs (Top 50) ---
                    u.auditLogs
                        .OrderByDescending(a => a.createdAt)
                        .Take(50)
                        .Select(a => new AuditLogDto(
                            a.auditID,
                            a.action,
                            a.result,
                            a.ip,
                            a.userAgent,
                            a.createdAt,
                            a.detail
                        ))
                        .ToList(),

                    // --- Email Verifications ---
                    u.emailVerifications
                        .OrderByDescending(ev => ev.createdAt)
                        .Select(ev => new EmailVerificationDto(
                            ev.emailVerificationID,
                            ev.createdAt,
                            ev.expiresAt,
                            ev.consumedAt
                        ))
                        .ToList(),

                    // --- Password Resets (Top 3) ---
                    u.passwordResets
                        .OrderByDescending(pr => pr.createdAt)
                        .Take(3)
                        .Select(pr => new PasswordResetDto(
                            pr.passwordResetID,
                            pr.createdAt,
                            pr.expiresAt,
                            pr.consumedAt
                        ))
                        .ToList(),

                    // --- Refresh Tokens (Top 20) ---
                    u.refreshTokens
                        .OrderByDescending(rt => rt.Created)
                        .Take(20)
                        .Select(rt => new RefreshTokenDto(
                            rt.refreshTokenID,
                            rt.sessionID,
                            rt.Created,
                            rt.Expires,
                            rt.Revoked,
                            rt.ReplacedByToken,
                            rt.Revoked == null && rt.Expires > now // IsActive logic
                        ))
                        .ToList(),

                    // --- Roles (Updated DTO) ---
                    u.userRoles
                        .Where(ur => ur.role.scope == "user") // Lọc Role Scope
                        .Select(ur => new roleDto(
                            ur.role.roleID,
                            ur.role.roleName,
                            ur.role.roleDescription
                        ))
                        .Distinct()
                        .ToList(),

                    // --- Permissions (Updated DTO & Logic) ---
                    u.userRoles
                        .Where(ur => ur.role.scope == "user")       // 1. Lọc Role Scope
                        .SelectMany(ur => ur.role.rolePermissions)  // Flatten ra danh sách quyền
                        .Where(rp => rp.permission.scope == "user") // 2. Lọc Permission Scope
                        .Select(rp => new permissionDto(
                            rp.permission.permissionID,
                            rp.permission.permissionName,
                            rp.permission.code
                        ))
                        .Distinct() // Loại bỏ quyền trùng lặp (do user có nhiều role chứa cùng 1 quyền)
                        .ToList()
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<GetUserResponseDto?>> GetAllUserWhereScopeUserAsync(CancellationToken ct)
        {
            var user = await _db.authUsers
                .Where(u => u.scope == "user")
                .Select(u => new GetUserResponseDto
                {
                    userID = u.userID,
                    userName = u.userName,
                    email = u.email,
                    status = u.status,
                    isEmailVerified = u.isEmailVerified,
                    profile = u.profile == null ? null : new ProfileResponseDto
                    {
                        firstName = u.profile.firstName,
                        lastName = u.profile.lastName,
                        avatar = u.profile.avatar,
                        gender = u.profile.gender,
                        dateOfBirth = u.profile.dateOfBirth
                    }
                })
                .ToListAsync(ct);
            return user;
        }


        public async Task<GetUserResponseDto?> GetUserByIDAsync(int userID, CancellationToken ct)
        {
            var user = await _db.authUsers
                .Where(u => u.userID == userID)
                .Select(u => new GetUserResponseDto
                {
                    userID = u.userID,
                    userName = u.userName,
                    email = u.email,
                    status = u.status,
                    isEmailVerified = u.isEmailVerified,
                    profile = u.profile == null ? null : new ProfileResponseDto
                    {
                        firstName = u.profile.firstName,
                        lastName = u.profile.lastName,
                        avatar = u.profile.avatar,
                        gender = u.profile.gender,
                        dateOfBirth = u.profile.dateOfBirth
                    }
                })
                .FirstOrDefaultAsync(ct);
            return user;
        }
        public async Task<List<GetUserResponseDto?>> GetAllUserAsync(CancellationToken ct)
        {
            var user = await _db.authUsers
                .Select(u => new GetUserResponseDto
                {
                    userID = u.userID,
                    userName = u.userName,
                    email = u.email,
                    status = u.status,
                    isEmailVerified = u.isEmailVerified,
                    profile = u.profile == null ? null : new ProfileResponseDto
                    {
                        firstName = u.profile.firstName,
                        lastName = u.profile.lastName,
                        avatar = u.profile.avatar,
                        gender = u.profile.gender,
                        dateOfBirth = u.profile.dateOfBirth
                    }
                })
                .ToListAsync(ct);
            return user;
        }

        public Task<AuthUser> DeleteUser( int id ,CancellationToken ct)
        {  
            var user = _db.authUsers.FirstOrDefault(x => x.userID == id);
            if (user != null)
            {
                _db.authUsers.Remove(user);
            }
            return Task.FromResult(user);
        }

        public Task<AuthUser?> FindByGoogleSub (string googleSub, CancellationToken ct)
            => _db.authUsers.FirstOrDefaultAsync(x => x.googleSub == googleSub, ct);

        public Task<AuthUser?> FindByIdAsync(int userId, CancellationToken ct)
            => _db.authUsers.FirstOrDefaultAsync(x => x.userID == userId, ct);

        public Task<AuthUser?> FindByUserNameAsync(string userName, CancellationToken ct)
            => _db.authUsers.FirstOrDefaultAsync(x => x.userName == userName, ct);

        public Task<AuthUser?> FindByEmailAsync(string email, CancellationToken ct)
            => _db.authUsers.FirstOrDefaultAsync(x => x.email == email, ct);

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
            => _db.authUsers.AnyAsync(x => x.email == email, ct);

        public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct)
            => _db.authUsers.AnyAsync(x => x.userName == userName, ct);

        public Task AddAsync(AuthUser user, CancellationToken ct)
            => _db.authUsers.AddAsync(user, ct).AsTask();

        public Task UpdateAsync(AuthUser user, CancellationToken ct)
        {
            _db.authUsers.Update(user);
            return Task.CompletedTask;
        }

        public async Task<bool> CheckUserScopeAsync(int userId, string scope, CancellationToken ct)
        {
            return await _db.authUsers.AnyAsync(u => u.userID == userId && u.scope == scope, ct);
        }
    }

    public sealed class ProfileRepository : IProfileRepository
    {
        private readonly AuthDbContext _db;
        public ProfileRepository(AuthDbContext db) => _db = db;

        public Task AddAsync(AuthProfile profile, CancellationToken ct)
            => _db.authProfiles.AddAsync(profile, ct).AsTask();

        public Task<AuthProfile?> GetByUserIdAsync(int userId, CancellationToken ct)
            => _db.authProfiles.FirstOrDefaultAsync(p => p.userID == userId, ct);

        public Task<bool> ExistsByUserIdAsync(int userId, CancellationToken ct)
            => _db.authProfiles.AnyAsync(p => p.userID == userId, ct);

        public Task UpdateAsync(AuthProfile profile, CancellationToken ct)
        {
            _db.authProfiles.Update(profile);
            return Task.CompletedTask;
        }

      
    }

}
