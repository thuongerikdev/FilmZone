using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Dtos.Role;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Implements.Role
{
    public  class AuthRoleService : IAuthRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _uow;
        public AuthRoleService(IRoleRepository roleRepository, IUnitOfWork uow)
        {
            _roleRepository = roleRepository;
            _uow = uow;
        }

        public async Task<ResponseDto<List<RoleResponse>>> GetAllRolesAsync(CancellationToken ct)
        {
            var roles = await _roleRepository.GetAllRolesAsync(ct);
            var rolesDto = roles.Select(r => new RoleResponse
            {
                roleID = r.roleID,
                roleName = r.roleName,
                roleDescription = r.roleDescription,
                scope = r.scope,
                isDefault = r.isDefault
            }).ToList();
            return ResponseConst.Success("Lấy danh sách thành công", rolesDto);

        }
        public async Task<string?> GetDefaultRoleAsync(CancellationToken ct)
        {
            var role = await _roleRepository.GetDefaultRoleAsync(ct);
            return role?.roleName;
        }
        public async Task<bool> RoleExistsAsync(string roleName, CancellationToken ct)
        {
            var role = await _roleRepository.GetRoleByNameAsync(roleName, ct);
            return role != null;
        }

        public  async Task<ResponseDto<RoleResponse>> UpdateRoleAsync(UpdateRoleRequest req, CancellationToken ct)
        {
            var existingRole = await _roleRepository.GetRoleByIdAsync(req.roleID, ct);
            if (existingRole == null)
            {
                throw new InvalidOperationException("Role not found.");
            }
            existingRole.roleName = req.roleName;
            existingRole.roleDescription = req.roleDescription;
            existingRole.isDefault = req.isDefault;
            existingRole.scope = req.scope;
            await _roleRepository.UpdateRoleAsync(existingRole, ct);
            await _uow.SaveChangesAsync(ct);
            var roleDto = new RoleResponse
            {
                roleName = existingRole.roleName,
                roleDescription = existingRole.roleDescription,
                isDefault = existingRole.isDefault
            };
            return ResponseConst.Success("Cập nhật vai trò thành công", roleDto);
        }

        public async Task<ResponseDto<bool>> DeleteRoleAsync(int roleID, CancellationToken ct)
        {
            var existingRole = await _roleRepository.GetRoleByIdAsync(roleID, ct);
            if (existingRole == null)
            {
                throw new InvalidOperationException("Role not found.");
            }
            await _roleRepository.DeleteRoleAsync(roleID, ct);
            await _uow.SaveChangesAsync(ct);
            return ResponseConst.Success("Xoá vai trò thành công", true);
        }

        public async Task<ResponseDto<RoleResponse>> GetRoleByIdAsync(int roleID, CancellationToken ct)
        {
            var role = await _roleRepository.GetRoleByIdAsync(roleID, ct);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found.");
            }
            var roleDto = new RoleResponse
            {
                roleID = role.roleID,
                roleName = role.roleName,
                roleDescription = role.roleDescription,
                isDefault = role.isDefault
            };
            return ResponseConst.Success("Lấy vai trò thành công", roleDto);
        }

        public async Task<ResponseDto<List<RoleResponse>>> GetRoleByUserID(int userID, CancellationToken ct)
        {
            var roles = await _roleRepository.GetRoleByUserID(userID, ct);
            var rolesDto = roles.Select(r => new RoleResponse
            {
                roleID = r.roleID,
                roleName = r.roleName,
                roleDescription = r.roleDescription,
                isDefault = r.isDefault
            }).ToList();
            return ResponseConst.Success("Lấy vai trò của user thành công", rolesDto);
        }




        public async Task<ResponseDto<RoleResponse>> AddRoleAsync(AddRoleRequest addRole, CancellationToken ct)
        {
            var existingRole = await _roleRepository.GetRoleByNameAsync(addRole.roleName, ct);
            if (existingRole != null)
            {
                throw new InvalidOperationException("Role already exists.");
            }
            var newRole = new Domain.Role.AuthRole
            {
                roleID =existingRole.roleID,
                roleName = addRole.roleName,
                isDefault = addRole.isDefault,
                roleDescription = addRole.roleDescription,
                scope = addRole.scope


            };
            await _roleRepository.AddRoleAsync(newRole, ct);
            await _uow.SaveChangesAsync(ct);
            var roleDto = new RoleResponse
            {
                roleID = newRole.roleID,
                roleName = newRole.roleName,
                roleDescription = newRole.roleDescription,
                isDefault = newRole.isDefault,
                scope = newRole.scope
            };
            return ResponseConst.Success("Thêm vai trò thành công", roleDto);
        }
    }
}
