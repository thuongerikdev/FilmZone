using FZ.Auth.Domain.MFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuthAuditLog authAuditLog, CancellationToken ct);
    }
    //public interface IMFARepository
    //{
    //    Task AddMFAAsync(AuthMfaSecret authMFA, CancellationToken ct);
    //    Task <bool> CheckEnabledMFAAsync(int userId, CancellationToken ct);
    //    Task<AuthMfaSecret?> GetByUserIdAsync(int userId, CancellationToken ct);
    //    //Task UpdateMFAAsync(AuthMfaSecret authMFA, CancellationToken ct);

    //    Task<AuthMfaSecret?> GetByUserAsync(int userId, CancellationToken ct);
    //    Task UpsertAsync(AuthMfaSecret entity, CancellationToken ct);


    //}

    public interface IMFARepository
    {
        Task<AuthMfaSecret?> GetByUserAsync(int userId, CancellationToken ct);
        Task UpsertAsync(AuthMfaSecret entity, CancellationToken ct);
        Task<bool> CheckEnabledMFAAsync(int userId, CancellationToken ct); // đã có từ code cũ
    }




    public interface IAuthUserSessionRepository
    {
        Task AddSessionAsync(AuthUserSession session, CancellationToken ct);
        Task<AuthUserSession?> FindByIdAsync(int sessionId, CancellationToken ct);
        Task MarkRevokedAsync(int sessionId, CancellationToken ct);

        // NEW: revoke tất cả session của 1 user, trả về số bản ghi bị ảnh hưởng
        Task<int> MarkAllRevokedForUserAsync(int userId, CancellationToken ct);
    }





    public interface IDeviceIdProvider
    {
        string GetOrCreate();
    }

}
