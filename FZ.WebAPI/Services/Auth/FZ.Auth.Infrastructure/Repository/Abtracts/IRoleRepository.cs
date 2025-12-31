
using FZ.Auth.Domain.Role;
using FZ.Auth.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IRoleRepository
    {
        Task AddRoleAsync(AuthRole role, CancellationToken ct);
        Task<List<AuthRole>> GetAllRolesAsync(CancellationToken ct);
        Task<AuthRole?> GetRoleByIdAsync(int roleID, CancellationToken ct);
        Task UpdateRoleAsync(AuthRole role, CancellationToken ct);
        Task<AuthRole?> GetRoleByNameAsync(string roleName, CancellationToken ct);
        Task DeleteRoleAsync(int roleID, CancellationToken ct);
        Task<AuthRole?> GetDefaultRoleAsync(CancellationToken ct);

        Task<List<AuthRole>> GetRoleByUserID(int userID, CancellationToken ct);
        Task<List<AuthRole>> GetRolesByIdsAsync(IEnumerable<int> roleIds, CancellationToken ct);
        Task<bool> CheckRoleScopeAsync(int roleId, string scope, CancellationToken ct);

        Task<bool> AreAllRolesInScopeAsync(List<int> roleIds, string scope, CancellationToken ct);


    }
    public interface IUserRoleRepository
    {
        Task AddUserRoleAsync(AuthUserRole userRole, CancellationToken ct);
        Task RemoveUserRoleAsync(int userID, int roleID, CancellationToken ct);
        Task<List<AuthUserRole>> GetUserRolesByUserIdAsync(int userId, CancellationToken ct);
        //Task<bool> UserHasRoleAsync(int userID, string roleName, CancellationToken ct);

        Task AddRangeUserRoleAsync(List<AuthUserRole> userRoles, CancellationToken ct);

        Task RemoveRangeUserRoleAsync(List<AuthUserRole> userRoles, CancellationToken ct);
    }



}
