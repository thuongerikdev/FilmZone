using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Dtos.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Auth
{
    [Route("roles")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IAuthRoleService _roleService;
        public RoleController(IAuthRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var result = await _roleService.GetAllRolesAsync(ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync(AddRoleRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _roleService.AddRoleAsync(req, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ErrorCode = 400, ex.Message });
            }
        }
        [HttpPut("updateRole")]
        public async Task<IActionResult> UpdateRoleAsync(UpdateRoleRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _roleService.UpdateRoleAsync(req, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ErrorCode = 400, ex.Message });
            }
        }
        [HttpDelete("deleteRole/{roleID}")]
        public async Task<IActionResult> DeleteRoleAsync(int roleID, CancellationToken ct)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(roleID, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ErrorCode = 400, ex.Message });
            }
        }
        [HttpGet("getRoleByUserID/{userID}")]
        public async Task<IActionResult> GetRoleByUserID(int userID, CancellationToken ct)
        {
            try
            {
                var result = await _roleService.GetRoleByUserID(userID, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ErrorCode = 400, ex.Message });
            }
        }

        //[ApiController]
        //[Route("api/users")]
        //public class UserController : ControllerBase
        //{
        //    // Cách 1: Dùng Policy (đã định nghĩa trong AuthStartUp)
        //    // Startup: options.AddPolicy("UserCreate", p => p.RequireClaim("permission", "USER_CREATE"));
        //    [HttpPost]
        //    [Authorize(Policy = "UserCreate")]
        //    public IActionResult CreateUser()
        //    {
        //        return Ok("Created");
        //    }

        //    // Cách 2: Chặn cứng Role (Không khuyến khích bằng Permission)
        //    [HttpDelete("{id}")]
        //    [Authorize(Roles = "Admin,SuperAdmin")]
        //    public IActionResult DeleteUser(int id)
        //    {
        //        return Ok("Deleted");
        //    }

        //    // Cách 3: Kiểm tra thủ công trong code (nếu logic phức tạp)
        //    [HttpGet("complex-check")]
        //    [Authorize]
        //    public IActionResult ComplexCheck()
        //    {
        //        // User.HasClaim check trực tiếp trong ClaimsPrincipal (được bung ra từ Token)
        //        if (!User.HasClaim(c => c.Type == "permission" && c.Value == "SPECIAL_REPORT"))
        //        {
        //            return Forbid();
        //        }
        //        return Ok("Report Data");
        //    }
        //}


    }
}
