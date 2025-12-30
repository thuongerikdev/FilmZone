using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.ApplicationService.MFAService.Implements.User;
using FZ.Auth.Dtos.User;
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
        [HttpGet("getAllUsersSlim")]
        [Authorize(Policy = "UserReadList")]
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
        [Authorize(Policy = "UserDelete")]
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

            var result = await _userService.GetUserByIDAsync(userId, ct);
            if (result.ErrorCode != 200) return StatusCode(result.ErrorCode, result);
            return Ok(result);
        }

        [HttpGet("getUserById")]
        [Authorize(Policy = "UserReadDetails")]
        public async Task<IActionResult> GetUserById([FromQuery] int userId, CancellationToken ct)
        {
            var result = await _userService.GetUserByIDAsync(userId, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }
        [HttpGet("GetUserSlimById{userID}")]
        [Authorize(Policy = "UserReadDetails")]
        public async Task<IActionResult> GetUserSlimById([FromQuery] int userID,  CancellationToken ct)
        {
            var result =  await _userService.GetSlimUserByID(userID, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("getAllUsers")]
        [Authorize(Policy = "UserReadDetails")]
        public async Task<IActionResult> GetAllUsersFull(CancellationToken ct)
        {
            var result = await _userService.GetAllUserAsync(ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }



        [HttpPut("update/profile")]
        [Authorize(Policy = "UserUpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] AuthUpdateProfileRequest req, CancellationToken ct)
        {

            var result = await _userService.AuthUpdateProfileRequest(req, ct);
            if (result.ErrorCode != 200) return StatusCode(result.ErrorCode, result);
            return Ok(result);
        }
        [HttpPut("update/username")]
        [Authorize(Policy = "UserUpdateProfile")]
        public async Task<IActionResult> UpdateUsername([FromQuery] int userId, [FromQuery] string newUsername, CancellationToken ct)
        {
            var result = await _userService.AuthUpdateUserName(userId, newUsername, ct);
            if (result.ErrorCode != 200) return StatusCode(result.ErrorCode, result);
            return Ok(result);
        }
    }
}
