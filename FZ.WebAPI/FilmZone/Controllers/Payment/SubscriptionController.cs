using FZ.Auth.ApplicationService.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/payment/subscription")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        [HttpGet("{subscriptionID}")]
        [Authorize(Policy = "SubscriptionReadOwn")]
        public async Task<IActionResult> GetSubscriptionByID(int subscriptionID, CancellationToken ct)
        {
            var response = await _subscriptionService.GetSubscriptionByID(subscriptionID, ct);
            if (response.ErrorCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.ErrorCode, response);
            }

        }
        [HttpGet("user/{userID}")]
        [Authorize(Policy = "SubscriptionReadOwn")]
        public async Task<IActionResult> GetSubscriptionByUserID(int userID, CancellationToken ct)
        {
            var response = await _subscriptionService.GetSubscriptionByUserID(userID, ct);
            if (response.ErrorCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.ErrorCode, response);

            }
        }
        [HttpGet("all")]
        [Authorize(Policy = "SubscriptionReadAll")]
        public async Task<IActionResult> GetAllSubscription()
        {
            var response = await _subscriptionService.GetAllSubscription();
            if (response.ErrorCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.ErrorCode, response);
            }

        }
        [HttpPost("expire-due")]
        [Authorize(Policy = "SubscriptionManage")]
        public async Task<IActionResult> ExpireIfDueAsync(CancellationToken ct)
        {
            await _subscriptionService.ExpireIfDueAsync(ct);
            return Ok(new { Message = "Expire check completed." });
        }

        [HttpPost("cancel-subs")]
        [Authorize(Policy = "SubscriptionCancel")]
        public async Task<IActionResult> CancelSubscriptionAsync(int userID, CancellationToken ct)
        {
            var response = await _subscriptionService.CancelSubscriptionAsync(userID, ct);
            if (response.ErrorCode == 200)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.ErrorCode, response);
            }
        }
    }
}
