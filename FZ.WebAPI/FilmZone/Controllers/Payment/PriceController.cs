using FZ.Auth.ApplicationService.Billing;
using FZ.Auth.Dtos.Billing;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/price")]
    public class PriceController : Controller
    {
        private readonly IPriceService _prices;
        public PriceController(IPriceService prices) { _prices = prices; }

        [HttpPost("Create")]
        public async Task<IActionResult> CreatePrice([FromBody] CreatePriceRequestDto dto, CancellationToken ct)
        {
           try 
           {
               var res = await _prices.CreatePrice(dto, ct);
               return StatusCode(res.ErrorCode == 200 ? 200 : 400, res);
           }
           catch (Exception ex)
           {
               return StatusCode(500, new { ErrorCode = 500, Message = "Internal server error", Details = ex.Message });
           }


        }
        [HttpPut("Update")]
        public async Task<IActionResult> UpdatePrice([FromBody] UpdatePriceRequestDto dto, CancellationToken ct)
        {
            try
            {
                var res = await _prices.UpdatePrice(dto, ct);
                return StatusCode(res.ErrorCode == 200 ? 200 : 400, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorCode = 500, Message = "Internal server error", Details = ex.Message });
            }
        }
        [HttpDelete("Delete/{priceID}")]
        public async Task<IActionResult> DeletePrice([FromRoute] int priceID, CancellationToken ct)
        {
            try
            {
                var res = await _prices.DeletePrice(priceID, ct);
                return StatusCode(res.ErrorCode == 200 ? 200 : 400, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorCode = 500, Message = "Internal server error", Details = ex.Message });
            }
        }
        [HttpGet("{priceID}")]
        public async Task<IActionResult> GetPrice([FromRoute] int priceID, CancellationToken ct)
        {
            try
            {
                var res = await _prices.GetPrice(priceID, ct);
                return StatusCode(res.ErrorCode == 200 ? 200 : 400, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorCode = 500, Message = "Internal server error", Details = ex.Message });
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPrices(CancellationToken ct)
        {
            try
            {
                var res = await _prices.GetAllPrices(ct);
                return StatusCode(res.ErrorCode == 200 ? 200 : 400, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErrorCode = 500, Message = "Internal server error", Details = ex.Message });
            }
        }
    }
}
