using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.ApplicationService.MFAService.Implements.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FZ.WebAPI.Controllers.Auth
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IAuthUserService _userService;
        public UserController(IAuthUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers(CancellationToken ct)
        {
            var result = await _userService.GetAllSlimAsync(ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUserAsync([FromQuery] int userId, CancellationToken ct)
        {
            var result = await _userService.DeleteUserAsync(userId, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var userIdStr = User.FindFirst("userId")?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { error = "No user id in token" });

            var result = await _userService.GetUserByIDAsync(userId , ct);
            if (result.ErrorCode != 200) return StatusCode(result.ErrorCode, result);
            return Ok(result);
        }

    }
}
