using FZ.Auth.ApplicationService.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/payment/invoice")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        [HttpGet("{orderID}")]
        [Authorize(Policy = "InvoiceReadOwn")]

        public async Task<IActionResult> GetInvoiceByOrderID(int orderID, CancellationToken ct)
        {
            var response = await _invoiceService.GetInvoiceByOrderID(orderID, ct);
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
        [Authorize(Policy = "InvoiceReadAll")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var response = await _invoiceService.GetAllInvoices();
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
        [Authorize(Policy = "InvoiceReadOwn")]
        public async Task<IActionResult> GetByUserID(int userID, CancellationToken ct)
        {
            var response = await _invoiceService.GetByUserID(userID, ct);
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
