using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IEmailTokenRepository
    {
        Task AddTokenAsync(AuthEmailVerification authEmail, CancellationToken ct);
        Task UpdateTokenAsync(AuthEmailVerification authEmail, CancellationToken ct);
        Task<AuthEmailVerification?> verifyEmail(VerifyEmailRequest verifyEmailDto, CancellationToken ct);
    }

    public interface IPasswordResetRepository
    {
        Task AddAsync(AuthPasswordReset entity, CancellationToken ct);
        Task<AuthPasswordReset?> FindLatestActiveAsync(int userId, string purpose, CancellationToken ct);
        Task UpdateAsync(AuthPasswordReset entity, CancellationToken ct);
        Task<List<AuthPasswordReset>> GetRecentAsync(int userId, string purpose, int take, CancellationToken ct);
    }

    public interface ITokenGenerate
    {
        string? GetClientIp();
        string? GetUserAgent();

        // 1. Login cũ (tự tạo session)
        Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user, string? createdByIp, TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        // 2. Login có sessionID (nhưng tự query quyền trong DB)
        Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user, int sessionId, string? createdByIp,
            TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        // 3. Login có sessionID + Danh sách quyền (Performance optimization & Logic injection)
        Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user,
            int sessionId,
            string createdByIp,
            TimeSpan accessTtl,
            TimeSpan refreshTtl,
            List<string> permissions // <--- Method mới bạn cần
        );

        // 4. Refresh Token
        Task<(string accessToken, AuthRefreshToken newRefresh)> RotateAsync(
            string incomingRefreshToken, string? ip,
            TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        // 5. Revoke
        Task RevokeAsync(string refreshToken, string? ip);
        Task<int> RevokeBySessionAsync(int userId, int sessionId, string? ip);
        Task<int> RevokeAllForUserAsync(int userId, string? ip);

        Task<(AuthRefreshToken token, AuthUser user)?> GetActiveAsync(string refreshToken);
        Task<AuthRefreshToken> GenerateUniqueRefreshTokenAsync(int userId, int sessionId, string? ip, TimeSpan ttl);
        Task AddTokenAsync(AuthRefreshToken token, CancellationToken ct);
    }
}