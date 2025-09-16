using FZ.Auth.Dtos.Payment;
using FZ.Auth.Infrastructure.Repository.Billing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.Billing.PaymentModule
{

    public interface IVnPayService
    {
        Task<string> CreatePaymentUrl(int orderiD, HttpContext context , CancellationToken ct);
        PaymentResponseModel PaymentExecute(IQueryCollection Collections);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly IOrderRepository _orders;

        public VnPayService(IConfiguration config, IOrderRepository orders)
        {
            _config = config; _orders = orders;
        }

        public async Task<string?> CreatePaymentUrl(int orderId, HttpContext ctx, CancellationToken ct)
        {
            var order = await _orders.GetByIdAsync(orderId, ct);
            if (order == null || order.status is "paid" or "confirmed") return null;

            var vnp = new VnPayLibrary();
            var scheme = ctx.Request.Scheme;
            var host = ctx.Request.Host.Value;
            var returnUrl = $"{scheme}://{host}/api/payment/vnpay/callback";

            long amountX100 = (long)(order.amount * 100m);

            vnp.AddRequestData("vnp_Version", _config["Vnpay:Version"]);
            vnp.AddRequestData("vnp_Command", _config["Vnpay:Command"]);
            vnp.AddRequestData("vnp_TmnCode", _config["Vnpay:TmnCode"]);
            vnp.AddRequestData("vnp_Amount", amountX100.ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", _config["Vnpay:CurrCode"]);
            vnp.AddRequestData("vnp_IpAddr", vnp.GetIpAddress(ctx));
            vnp.AddRequestData("vnp_Locale", _config["Vnpay:Locale"] ?? "vn");
            vnp.AddRequestData("vnp_OrderInfo", $"Pay order {orderId}");
            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);

            // KEY: dùng OrderId làm TxnRef
            vnp.AddRequestData("vnp_TxnRef", orderId.ToString());

            var paymentUrl = vnp.CreateRequestUrl(_config["Vnpay:BaseUrl"], _config["Vnpay:HashSecret"]);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection query)
        {
            var vnp = new VnPayLibrary();
            return vnp.GetFullResponseData(query, _config["Vnpay:HashSecret"]);
        }
    }
}
