using FZ.Auth.ApplicationService.Billing;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/payment/order")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet("{orderID}")]
        public async Task<IActionResult> GetOrderByID(int orderID, CancellationToken ct)
        {
            var response = await _orderService.GetOrderByID(orderID, ct);
            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(new { Message = "Order not found" });
            }

        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrder()
        {
            var response = await _orderService.GetAllOrder();
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
        public async Task<IActionResult> GetOrdersByUserID(int userID, CancellationToken ct)
        {
            var response = await _orderService.GetOrdersByUserID(userID, ct);
            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(new { Message = "Orders not found for the user" });
            }
        }

    }
}
