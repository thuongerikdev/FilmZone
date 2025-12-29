// FZ.Auth.ApplicationService.Service.Implements.Role/AuthUserRoleService.cs

using FZ.Auth.ApplicationService.Common;
using FZ.Auth.Domain.Role; // Import Entity
using FZ.Auth.Dtos.Role;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Implements; // Import Repo Interface
using FZ.Constant;
using Microsoft.Extensions.Logging;

namespace FZ.Auth.ApplicationService.Service.Implements.Role
{
    public interface IAuthUserRoleService
    {
        Task<ResponseDto<bool>> AddUserRoleAsync(UserRoleRequestDto req, CancellationToken ct);
    }
    public class AuthUserRoleService : AuthServiceBase, IAuthUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _uow;

        public AuthUserRoleService(
            ILogger<AuthUserRoleService> logger,
            IUnitOfWork uow,
            IUserRoleRepository userRoleRepository) : base(logger)
        {
            _uow = uow;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<ResponseDto<bool>> AddUserRoleAsync(UserRoleRequestDto req, CancellationToken ct)
        {
            try
            {
                // 1. Validate (Cho phép list rỗng nếu muốn xóa hết quyền của user)
                if (req.roleIDs == null) req.roleIDs = new List<int>();

                // 2. Lấy hiện trạng từ DB
                var currentRoles = await _userRoleRepository.GetUserRolesByUserIdAsync(req.userID, ct);

                // Input mong muốn (Distinct để tránh trùng lặp trong request)
                var targetRoleIds = req.roleIDs.Distinct().ToHashSet();

                // 3. Phân loại (Set Operation)

                // A. Cần THÊM: Có trong Input nhưng chưa có trong DB
                var rolesToAdd = new List<AuthUserRole>();
                foreach (var id in targetRoleIds)
                {
                    // Nếu DB chưa có ID này -> Thêm vào list Add
                    if (!currentRoles.Any(x => x.roleID == id))
                    {
                        rolesToAdd.Add(new AuthUserRole
                        {
                            userID = req.userID,
                            roleID = id,
                            assignedAt = DateTime.UtcNow
                        });
                    }
                }

                // B. Cần XÓA: Có trong DB nhưng không còn trong Input
                var rolesToRemove = currentRoles
                    .Where(x => !targetRoleIds.Contains(x.roleID))
                    .ToList();

                // 4. Thực thi thay đổi
                if (rolesToAdd.Any())
                {
                    await _userRoleRepository.AddRangeUserRoleAsync(rolesToAdd, ct);
                }

                if (rolesToRemove.Any())
                {
                    await _userRoleRepository.RemoveRangeUserRoleAsync(rolesToRemove, ct);
                }

                // 5. Nếu không có gì thay đổi
                if (!rolesToAdd.Any() && !rolesToRemove.Any())
                {
                    return ResponseConst.Success("No changes needed (user roles are already up to date).", true);
                }

                // 6. Lưu xuống DB
                await _uow.SaveChangesAsync(ct);

                return ResponseConst.Success(
                    $"Synced successfully: Added {rolesToAdd.Count}, Removed {rolesToRemove.Count} roles.",
                    true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing roles for UserID: {UserID}", req.userID);
                return ResponseConst.Error<bool>(500, "An error occurred while syncing roles.");
            }
        }
    }
}