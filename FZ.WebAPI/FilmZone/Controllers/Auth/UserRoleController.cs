using FZ.Auth.ApplicationService.Service.Implements.Role;
using FZ.Auth.Dtos.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Auth
{
    [Route("user-roles")]
    [ApiController]
    public class UserRoleController : Controller
    {
        private readonly IAuthUserRoleService _authUserRoleService;
        public UserRoleController(IAuthUserRoleService authUserRoleService)
        {
            _authUserRoleService = authUserRoleService;
        }
        [HttpPost("assign-roles")]
        [Authorize(Policy = "RoleAssign")]
        public async Task<IActionResult> AssignRolesToUser([FromBody] UserRoleRequestDto req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authUserRoleService.AddUserScopeUserRoleAsync(req, ct);
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
        [HttpPost("admin/assign-roles")]
        [Authorize(Policy = "RoleAssignAdmin")]
        public async Task<IActionResult> AssignRolesToUserAdmin([FromBody] UserRoleRequestDto req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authUserRoleService.AddUserRoleAsync(req, ct);
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
    }
}
