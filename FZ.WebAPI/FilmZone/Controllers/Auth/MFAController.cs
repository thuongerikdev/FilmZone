//using FZ.Auth.ApplicationService.MFAService.Abtracts;
//using FZ.Auth.ApplicationService.MFAService.Implements.Account;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace FZ.WebAPI.Controllers.Auth
//{
//    [Route("mfa")]
//    [ApiController]
//    [Authorize(Policy = "AuditLogManage")]
//    public class MFAController : Controller
//    {
//        private readonly IMfaService _mfaService;
//        public MFAController(IMfaService mfaService)
//        {
//            _mfaService = mfaService;
//        }
//        [HttpGet("getall")]
//        public async Task<IActionResult> GetAllMfaMethods(CancellationToken ct)
//        {
//            var result = await _mfaService.GetAllMFAAsync(ct);
//            if (result.ErrorCode != 200)
//            {
//                return BadRequest(result);
//            }
//            return Ok(result);
//        }
//        [HttpGet("getbymfaid/{mfaId}")]
//        public async Task<IActionResult> GetMfaByMfaId(int mfaId, CancellationToken ct)
//        {
//            var result = await _mfaService.GetByIdAsync(mfaId, ct);
//            if (result.ErrorCode != 200)
//            {
//                return BadRequest(result);
//            }
//            return Ok(result);

//        }
//        [HttpGet("getByUserId/{userID}")]
//        public async Task<IActionResult> GetMfaByUserId(int userID, CancellationToken ct)
//        {
//            var result = await _mfaService.GetByUserAsync(userID, ct);
//            if (result.ErrorCode != 200)
//            {
//                return BadRequest(result);
//            }
//            return Ok(result);
//        }
//    }
//}
