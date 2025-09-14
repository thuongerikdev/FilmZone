using FZ.Auth.ApplicationService.MFAService.Abtracts;
using Microsoft.AspNetCore.Mvc;

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
    }
}
