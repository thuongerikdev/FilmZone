using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.Billing
{
    public class CreateOrderRequest
    {
    }
    // FZ.Auth.Dtos/Payment/CheckoutDtos.cs
    public class CheckoutRequestDto
    {
        public int PriceId { get; set; }      // 101/102/103
        public bool AutoRenew { get; set; }
    }

    public class CheckoutResponseDto
    {
        public int OrderId { get; set; }
        public int InvoiceId { get; set; }
        public string PayUrl { get; set; } = default!;
    }

}
