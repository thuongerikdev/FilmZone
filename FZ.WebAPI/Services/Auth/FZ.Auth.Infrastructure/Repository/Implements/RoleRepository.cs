using FZ.Auth.Domain.Role;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Implements
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _db;
        public RoleRepository(AuthDbContext db) => _db = db;

        public Task AddRoleAsync(AuthRole role, CancellationToken ct)
            => _db.authRoles.AddAsync(role, ct).AsTask();

        public Task UpdateRoleAsync(AuthRole role, CancellationToken ct)
        {
            _db.authRoles.Update(role); // hoặc để EF tracking tự detect
            return Task.CompletedTask;
        }
        public Task<AuthRole?> GetRoleByNameAsync(string roleName, CancellationToken ct)
            => _db.authRoles.FirstOrDefaultAsync(x => x.roleName == roleName, ct);

        public Task<AuthRole?> GetRoleByIdAsync(int roleId, CancellationToken ct)
            => _db.authRoles.FirstOrDefaultAsync(x => x.roleID == roleId, ct);

        public Task<List<AuthRole>> GetAllRolesAsync(CancellationToken ct)
            => _db.authRoles.ToListAsync(ct);

        public async Task DeleteRoleAsync(int roleID, CancellationToken ct)
        {
            await _db.authRoles
                     .Where(x => x.roleID == roleID)
                     .ExecuteDeleteAsync(ct);
        }

        public Task<AuthRole?> GetRoleByIdWithUsersAsync(int roleID, CancellationToken ct)
            => _db.authRoles.Include(r => r.userRoles)
                            .ThenInclude(ur => ur.user)
                            .FirstOrDefaultAsync(r => r.roleID == roleID, ct);

        public Task<List<AuthRole>> GetRoleByUserID (int userID, CancellationToken ct)
            => _db.authUserRoles
                .Where(ur => ur.userID == userID)
                .Select(ur => ur.role)
               .ToListAsync(ct);


        public Task<AuthRole?> GetDefaultRoleAsync(CancellationToken ct)
            => _db.authRoles.FirstOrDefaultAsync(x => x.isDefault, ct);
    }

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AuthDbContext _db;
        public UserRoleRepository(AuthDbContext db) => _db = db;
        public Task AddUserRoleAsync(AuthUserRole userRole, CancellationToken ct)
            => _db.authUserRoles.AddAsync(userRole, ct).AsTask();
        public Task RemoveUserRoleAsync(int userID, int roleID, CancellationToken ct)
            => _db.authUserRoles.Where(x => x.userID == userID && x.roleID == roleID).ExecuteDeleteAsync(ct);
        public Task<List<AuthUserRole>> GetUserRolesByUserIdAsync(int userId, CancellationToken ct)
            => _db.authUserRoles.Where(x => x.userID == userId).ToListAsync(ct);

        public Task AddRangeUserRoleAsync(List<AuthUserRole> userRoles, CancellationToken ct)
            => _db.authUserRoles.AddRangeAsync(userRoles, ct);

        public Task RemoveRangeUserRoleAsync(List<AuthUserRole> userRoles, CancellationToken ct)
        {
            _db.authUserRoles.RemoveRange(userRoles);
            return Task.CompletedTask;
        }
        public async Task<List<AuthRole>> GetRolesByIdsAsync(IEnumerable<int> roleIds, CancellationToken ct)
        {
            // Dùng Contains để tạo câu lệnh SQL: WHERE roleID IN (...)
            return await _db.authRoles
                .Where(r => roleIds.Contains(r.roleID))
                .ToListAsync(ct);
        }
    }




}
