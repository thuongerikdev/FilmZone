using FZ.Auth.ApplicationService.Common;
using FZ.Auth.Domain.Role;
using FZ.Auth.Dtos.Role;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Implements;
using FZ.Constant;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.Service.Implements.Role
{
    public interface IAuthRolePermissionService
    {
        Task<ResponseDto<bool>> AddRolePermissionAsync(RolePermissionRequestDto req, CancellationToken ct);


    }
    public class AuthRolePermissionService : AuthServiceBase, IAuthRolePermissionService
    {
        // Lưu ý: UnitOfWork nên inject Interface IUnitOfWork thay vì Class UnitOfWork
        private readonly IUnitOfWork _authUnitOfWork;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public AuthRolePermissionService(
            IUnitOfWork authUnitOfWork,
            ILogger<AuthRolePermissionService> logger,
            IRolePermissionRepository rolePermissionRepository) : base(logger)
        {
            _authUnitOfWork = authUnitOfWork;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<ResponseDto<bool>> AddRolePermissionAsync(RolePermissionRequestDto req, CancellationToken ct)
        {
            try
            {
                // 1. Validate (Cho phép list rỗng nếu muốn xóa hết quyền của Role)
                if (req.permissionIDs == null) req.permissionIDs = new List<int>();

                // 2. Lấy danh sách hiện tại từ DB
                var currentPermissions = await _rolePermissionRepository.GetRolePermissionsByRoleIdAsync(req.roleID, ct);

                // Set mong muốn (Distinct để tránh trùng lặp trong request)
                var targetPermissionIds = req.permissionIDs.Distinct().ToHashSet();

                // 3. Phân loại (Set Operation)

                // A. Cần THÊM: Có trong Input nhưng chưa có trong DB
                var permissionsToAdd = new List<AuthRolePermission>();
                foreach (var id in targetPermissionIds)
                {
                    // Nếu DB chưa có ID này
                    if (!currentPermissions.Any(x => x.permissionID == id))
                    {
                        permissionsToAdd.Add(new AuthRolePermission
                        {
                            roleID = req.roleID,
                            permissionID = id
                        });
                    }
                }

                // B. Cần XÓA: Có trong DB nhưng không còn trong Input
                var permissionsToRemove = currentPermissions
                    .Where(x => !targetPermissionIds.Contains(x.permissionID))
                    .ToList();

                // 4. Thực thi thay đổi
                if (permissionsToAdd.Any())
                {
                    await _rolePermissionRepository.AddRangeRolePermissionAsync(permissionsToAdd, ct);
                }

                if (permissionsToRemove.Any())
                {
                    await _rolePermissionRepository.RemoveRangeRolePermissionAsync(permissionsToRemove, ct);
                }

                // 5. Nếu không có gì thay đổi
                if (!permissionsToAdd.Any() && !permissionsToRemove.Any())
                {
                    return ResponseConst.Success("No changes needed (role permissions are already up to date).", true);
                }

                // 6. Lưu xuống DB
                await _authUnitOfWork.SaveChangesAsync(ct);

                return ResponseConst.Success(
                    $"Synced successfully: Added {permissionsToAdd.Count}, Removed {permissionsToRemove.Count} permissions.",
                    true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing permissions for RoleID: {RoleId}", req.roleID);
                return ResponseConst.Error<bool>(500, "An error occurred while syncing role permissions.");
            }
        }
    }
}