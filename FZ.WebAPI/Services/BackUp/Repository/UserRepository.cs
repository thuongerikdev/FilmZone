using FZ.Auth.ApplicationService.Repository;
using FZ.Auth.Domain.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;
        public UserRepository(AuthDbContext db) => _db = db;

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
    }

    public sealed class ProfileRepository : IProfileRepository
    {
        private readonly AuthDbContext _db;
        public ProfileRepository(AuthDbContext db) => _db = db;

        public Task AddAsync(AuthProfile profile, CancellationToken ct)
            => _db.authProfiles.AddAsync(profile, ct).AsTask();
    }
}
