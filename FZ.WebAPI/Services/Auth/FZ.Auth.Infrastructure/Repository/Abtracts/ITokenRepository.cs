using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IEmailTokenRepository
    {
        Task AddTokenAsync(AuthEmailVerification authEmail , CancellationToken ct);

        Task UpdateTokenAsync(AuthEmailVerification authEmail, CancellationToken ct);

        Task<AuthEmailVerification?> verifyEmail(VerifyEmailRequest verifyEmailDto, CancellationToken ct);

    }
    //public interface IRefreshTokenRepository
    //{
      
        //Task<AuthRefreshToken?> GetByTokenAsync(string token, CancellationToken ct);
        //Task<List<AuthRefreshToken>> GetByUserIdAsync(int userId, CancellationToken ct);
        //Task UpdateTokenAsync(AuthRefreshToken token, CancellationToken ct);
        //Task RevokeDescendantTokensAsync(AuthRefreshToken token, string? ipAddress, string? reason, CancellationToken ct);
        //Task RemoveOldTokensAsync(int userId, DateTime olderThan, CancellationToken ct);
    //}
    //public interface IPasswordResetRepository
    //{
    //    Task AddAsync(AuthPasswordReset reset, CancellationToken ct);
    //    Task<AuthPasswordReset?> GetLatestActiveAsync(int userId, CancellationToken ct);
    //    Task<AuthPasswordReset?> VerifyAsync(int userId, string plainToken, CancellationToken ct);
    //    Task UpdateAsync(AuthPasswordReset reset, CancellationToken ct);
    //    Task InvalidateAllForUserAsync(int userId, CancellationToken ct); // optional an toàn
    //    public Task<AuthPasswordReset?> GetByIdAsync(int id, CancellationToken ct);
           
    //}

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

        Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user, int sessionId, string? createdByIp,
            TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        Task<(string accessToken, AuthRefreshToken refreshToken)> IssuePairAsync(
            AuthUser user, string? createdByIp, TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        Task<(string accessToken, AuthRefreshToken newRefresh)> RotateAsync(
            string incomingRefreshToken, string? ip,
            TimeSpan? accessTtl = null, TimeSpan? refreshTtl = null);

        Task RevokeAsync(string refreshToken, string? ip);

        // NEW: phục vụ logout theo session / logout all
        Task<int> RevokeBySessionAsync(int userId, int sessionId, string? ip);
        Task<int> RevokeAllForUserAsync(int userId, string? ip);

        Task<(AuthRefreshToken token, AuthUser user)?> GetActiveAsync(string refreshToken);

        Task<AuthRefreshToken> GenerateUniqueRefreshTokenAsync(int userId, int sessionId, string? ip, TimeSpan ttl);

        // Back-compat:
        Task AddTokenAsync(AuthRefreshToken token, CancellationToken ct);
    }
}
