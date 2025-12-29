using FZ.Auth.Domain.Role;
using FZ.Constant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Implements
{
    public interface IPermissionRepository
    {
        Task<List<AuthPermission>> GetPermissionsByUserIdAsync(int userId, CancellationToken ct);
        Task AddPermissionAsync(AuthPermission permission, CancellationToken ct);
        Task UpdatePermissionAsync(AuthPermission permission, CancellationToken ct);
        Task DeletePermissionAsync (AuthPermission permission, CancellationToken ct);
        Task<AuthPermission?> GetPermissionByNameAsync(string permissionName, CancellationToken ct);
        Task<List<AuthPermission>> GetAllPermissionsAsync(CancellationToken ct);
        Task<List<AuthPermission>> GettPermissionByRoleIdAsync(int roleId, CancellationToken ct);
        Task<AuthPermission> GetPermissionByIdAsync (int permissionId, CancellationToken ct);
        Task AddRangePermissionAsync(List<AuthPermission> permissions, CancellationToken ct);

    }
    public class PermissionRepository: IPermissionRepository
    {
        private readonly AuthDbContext _db;
        public PermissionRepository(AuthDbContext db) => _db = db;
        public Task<AuthPermission> GetPermissionByIdAsync(int permissionId, CancellationToken ct)
        {
            return _db.authPermissions.FirstAsync(p => p.permissionID == permissionId, ct);
        }
        public Task AddRangePermissionAsync(List<AuthPermission> permissions, CancellationToken ct)
        {
            return _db.authPermissions.AddRangeAsync(permissions, ct);
        }


        public async Task<List<AuthPermission>> GetPermissionsByUserIdAsync(int userId, CancellationToken ct)
        {
            // Join: User -> UserRole -> Role -> RolePermission -> Permission
            var query = await _db.Entry(_db.authUsers.Find(userId))
                .Collection(u => u.userRoles)
                .Query()
                .Include(ur => ur.role)
                .ThenInclude(r => r.rolePermissions)
                .ThenInclude(rp => rp.permission)
                .SelectMany(ur => ur.role.rolePermissions)
                .Select(rp => rp.permission)
                .ToListAsync();
            return query;

        }
        public Task AddPermissionAsync(AuthPermission permission, CancellationToken ct)
        {
            _db.authPermissions.Add(permission);
            return Task.CompletedTask;
        }
        public Task UpdatePermissionAsync(AuthPermission permission, CancellationToken ct)
        {
            _db.authPermissions.Update(permission);
            return Task.CompletedTask;
        }
        public Task<AuthPermission?> GetPermissionByNameAsync(string permissionName, CancellationToken ct)
        {
            return _db.authPermissions.FirstOrDefaultAsync(p => p.permissionName == permissionName, ct);
        }
        public Task DeletePermissionAsync(AuthPermission permission, CancellationToken ct)
        {
            _db.authPermissions.Remove(permission);
            return Task.CompletedTask;
        }
        public Task<List<AuthPermission>> GetAllPermissionsAsync(CancellationToken ct)
        {
            return _db.authPermissions.ToListAsync(ct);
        }
        public async Task<List<AuthPermission>> GettPermissionByRoleIdAsync(int roleId, CancellationToken ct)
        {
            var permissions = await _db.authRolePermissions
                .Where(rp => rp.roleID == roleId)
                .Include(rp => rp.permission)
                .Select(rp => rp.permission)
                .ToListAsync(ct);
            return permissions;
        }

    }
}
