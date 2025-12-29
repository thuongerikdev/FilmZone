using FZ.Auth.ApplicationService.Service.Implements.Role;
using FZ.Auth.Dtos.Role;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Auth
{
    [Route("permissions")]
    [ApiController]
    public class PermissionController : Controller
    {
        private readonly IAuthPermissionService _permissionService;
        public PermissionController(IAuthPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllPermissions(CancellationToken ct)
        {
            var result = await _permissionService.GetAllPermissionsAsync(ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("addPermission")]
        public async Task<IActionResult> AddPermissionAsync(CreatePermissionRequestDto req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _permissionService.CreatePermissionAsync(req, ct);
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
        [HttpPut("updatePermission")]
        public async Task<IActionResult> UpdatePermissionAsync(UpdatePermissionRequestDto req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _permissionService.UpdatePermissionAsync(req, ct);
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
        [HttpDelete("delate")]
        public async Task<IActionResult> DeletePermissionAsync([FromQuery] int permissionId, CancellationToken ct)
        {
            try
            {
                var result = await _permissionService.DeletePermissionAsync(permissionId, ct);
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
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetPermissionByIdAsync([FromQuery] int permissionId, CancellationToken ct)
        {
            var result = await _permissionService.GetPermissionByIdAsync(permissionId, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("getbyUserID/{ID}")]
        public async Task<IActionResult> GetPermissionByCodeAsync(int ID, CancellationToken ct)
        {
            var result = await _permissionService.GetPermissionsByUserIdAsync(ID, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("getbyRoleID/{ID}")]
        public async Task<IActionResult> GetPermissionByRoleIdAsync(int ID, CancellationToken ct)
        {
            var result = await _permissionService.GetPermissionByRoleIdAsync(ID, ct);
            if (result.ErrorCode != 200)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("BulkCreate")]
        public async Task<IActionResult> BulkCreatePermissionsAsync(List<CreatePermissionRequestDto> reqs, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var results = new List<object>();
            foreach (var req in reqs)
            {
                try
                {
                    var result = await _permissionService.CreatePermissionAsync(req, ct);
                    if (result.ErrorCode != 200)
                    {
                        results.Add(new { Request = req, Result = result });
                    }
                    else
                    {
                        results.Add(new { Request = req, Result = result });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    results.Add(new { Request = req, Error = ex.Message });
                }
            }
            return Ok(results);
        }
    }
}
