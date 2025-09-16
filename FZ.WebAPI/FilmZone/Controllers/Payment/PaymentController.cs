// FZ.WebAPI/Controllers/Payment/PaymentController.cs
using FZ.Auth.ApplicationService.Billing;
using FZ.Auth.ApplicationService.Billing.PaymentModule;
using FZ.Auth.Dtos.Billing;
using FZ.Auth.Dtos.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPay;
        private readonly IOrderService _orders;

        public PaymentController(IVnPayService vnPay, IOrderService orders)
        {
            _vnPay = vnPay; _orders = orders;
        }

        [HttpPost("vnpay/checkout")]
        [Authorize]
        public async Task<ActionResult<CheckoutResponseDto>> CheckoutVnPay([FromBody] CheckoutRequestDto dto, CancellationToken ct)
        {
            var uidClaim = User.FindFirst("uid") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (uidClaim == null || !int.TryParse(uidClaim.Value, out var userId)) return Unauthorized();

            var (order, invoice) = await _orders.CreateOrderAsync(userId, dto.PriceId, "vnpay", ct);
            var payUrl = await _vnPay.CreatePaymentUrl(order.orderID, HttpContext, ct);
            if (string.IsNullOrEmpty(payUrl)) return BadRequest("Cannot create payment url");

            return new CheckoutResponseDto { OrderId = order.orderID, InvoiceId = invoice.invoiceID, PayUrl = payUrl };
        }

        [HttpGet("vnpay/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback(CancellationToken ct)
        {
            var txnRef = Request.Query["vnp_TxnRef"].ToString();
            if (!int.TryParse(txnRef, out var orderId)) return HtmlError("Invalid order id", 400);

            var amountStr = Request.Query["vnp_Amount"].ToString(); // x100
            if (!long.TryParse(amountStr, out var amountX100)) return HtmlError("Invalid amount", 400);
            var paidAmount = (decimal)amountX100 / 100m;

            var providerPaymentId = Request.Query["vnp_TransactionNo"].ToString();

            var resp = _vnPay.PaymentExecute(Request.Query);
            if (resp.VnPayResponseCode == "00")
            {
                var ok = await _orders.MarkPaidAndActivateAsync(orderId, "vnpay", providerPaymentId, paidAmount, ct);
                if (!ok) return HtmlError("Order update failed", 500);
                return HtmlSuccess(orderId, paidAmount, "VNPAY");
            }
            await _orders.MarkFailedAsync(orderId, "vnpay", resp.VnPayResponseCode, ct);
            return HtmlError($"Payment failed, code: {resp.VnPayResponseCode}", 400);
        }

        private ContentResult HtmlError(string message, int code) => Content($@"
        <!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'/><title>Payment Error</title>
        <style>body{{font-family:Arial,sans-serif;text-align:center;padding:48px}}.error{{color:#c00}}</style></head>
        <body><h1>Payment Failed</h1><p class='error'>{message}</p><p>Error Code: {code}</p><a href='/'>Return Home</a></body></html>", "text/html");

        private ContentResult HtmlSuccess(int orderId, decimal amount, string method) => Content($@"
        <!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'/><title>Payment Success</title>
        <style>body{{font-family:Arial,sans-serif;text-align:center;padding:48px}}.ok{{color:#090}}.details{{margin-top:16px;display:inline-block;text-align:left}}</style></head>
        <body><h1>Payment Successful</h1><p class='ok'>Your payment has been processed successfully!</p>
        <div class='details'>
        <p><strong>Order ID:</strong> {orderId}</p>
        <p><strong>Total Amount:</strong> {amount:n0} VND</p>
        <p><strong>Payment Method:</strong> {method}</p>
        <p><strong>Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
        </div><a href='/'>Return Home</a></body></html>", "text/html");
    }
}
