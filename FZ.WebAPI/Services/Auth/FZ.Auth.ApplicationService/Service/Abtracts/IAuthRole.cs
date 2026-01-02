using FZ.Auth.Dtos.Role;
using FZ.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Abtracts
{
    public interface IAuthRoleService
    {
        Task<ResponseDto<List<RoleResponse>>> GetAllRolesAsync(CancellationToken ct);
        Task<ResponseDto<RoleResponse>> AddRoleAsync(AddRoleRequest req, CancellationToken ct);
        Task<ResponseDto<RoleResponse>> UpdateRoleAsync(UpdateRoleRequest req, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteRoleAsync(int roleID, CancellationToken ct);

        // ==========================================================
        Task<ResponseDto<RoleResponse>> AddRoleAsyncWhereScopeUser(AddRoleWhereScopeUserRequest req, CancellationToken ct);
        Task<ResponseDto<RoleResponse>> UpdateRoleAsyncWhereScopeUser(UpdateRoleWhereScopeUserRequest req, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteRoleAsyncWhereScopeUser(int roleID, CancellationToken ct);

        Task<ResponseDto<RoleResponse>> CloneRoleWhereScopeUserAsync(CloneUserRoleRequest req, CancellationToken ct);



        //Task<ResponseDto<bool>> AssignRoleToUserAsync(AssignRoleRequest req, CancellationToken ct);
        Task<ResponseDto<List<RoleResponse>>> GetRoleByUserID(int userID, CancellationToken ct);
        Task<ResponseDto<RoleResponse>> CloneRoleAsync(CloneRoleRequest req, CancellationToken ct);
    }
}
