namespace FZ.Auth.Infrastructure.Repository.Implements
{
    using FZ.Auth.Domain.Token;
    using FZ.Auth.Domain.User;
    using FZ.Auth.Infrastructure.Repository.Abtracts;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using StackExchange.Redis;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using IDatabase = StackExchange.Redis.IDatabase;

    public class TokenRepository : IEmailTokenRepository
    {
        public AuthDbContext _context;
        private readonly IPasswordHasher _hasher;

        public TokenRepository(AuthDbContext context, IPasswordHasher passwordHasher     )
        {
            _context = context;
            _hasher = passwordHasher;

        }
        public Task AddTokenAsync(Domain.Token.AuthEmailVerification authEmail, CancellationToken ct)
         => _context.authEmailVerifications.AddAsync(authEmail, ct).AsTask();

        public async Task<AuthEmailVerification?> verifyEmail(Dtos.User.VerifyEmailRequest dto, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // Lấy token CHƯA dùng, CÒN HẠN, mới nhất của user
            var candidate = await _context.authEmailVerifications
                .Where(x => x.userID == dto.userID
                         && x.consumedAt == null
                         && x.expiresAt > now)
                .OrderByDescending(x => x.createdAt)
                .FirstOrDefaultAsync(ct);

            if (candidate is null)
                return null;

            // So sánh token plain với salted hash đã lưu
            var ok = _hasher.Verify(dto.token, candidate.codeHash);
            return ok ? candidate : null; // KHÔNG ghi DB ở đây
        }





        public Task UpdateTokenAsync(Domain.Token.AuthEmailVerification authEmail, CancellationToken ct)
        {
            _context.authEmailVerifications.Update(authEmail); // hoặc để EF tracking tự detect
            return Task.CompletedTask;
        }
    }

    //public class RefreshTokenRepository : IRefreshTokenRepository
    //{
    //    private readonly AuthDbContext _db;
    //    public RefreshTokenRepository(AuthDbContext db) => _db = db;
    //    public Task AddTokenAsync(AuthRefreshToken token, CancellationToken ct)
    //        => _db.authRefreshTokens.AddAsync(token, ct).AsTask();
    //    public Task<AuthRefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
    //        => _db.authRefreshTokens.Include(t => t.user).FirstOrDefaultAsync(x => x.Token == token, ct);
    //    public Task<List<AuthRefreshToken>> GetByUserIdAsync(int userId, CancellationToken ct)
    //        => _db.authRefreshTokens.Where(x => x.userID == userId).ToListAsync(ct);
    //    public Task UpdateTokenAsync(AuthRefreshToken token, CancellationToken ct)
    //    {
    //        _db.authRefreshTokens.Update(token); // hoặc để EF tracking tự detect
    //        return Task.CompletedTask;
    //    }
    //    public async Task RevokeDescendantTokensAsync(AuthRefreshToken token, string? ipAddress, string? reason, CancellationToken ct)
    //    {
    //        var childToken = await _db.authRefreshTokens.FirstOrDefaultAsync(x => x.parentID == token.id && x.isRevoked == false, ct);
    //        if (childToken != null)
    //        {
    //            childToken.isRevoked = true;
    //            childToken.revokedAt = DateTime.UtcNow;
    //            childToken.revokedByIp = ipAddress;
    //            childToken.revocationReason = reason;
    //            await UpdateTokenAsync(childToken, ct);
    //            await RevokeDescendantTokensAsync(childToken, ipAddress, reason, ct);
    //        }
    //    }
    //    public async Task RemoveOldTokensAsync(int userId, DateTime olderThan, CancellationToken ct)
    //    {
    //        var oldTokens = await _db.authRefreshTokens
    //            .Where(x => x.userID == userId && x.Created < olderThan)
    //            .ToListAsync(ct);
    //        if (oldTokens.Any())
    //        {
    //            _db.authRefreshTokens.RemoveRange(oldTokens);
    //            await _db.SaveChangesAsync(ct);
    //        }
    //    }
    //}


}

