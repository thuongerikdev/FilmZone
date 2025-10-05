using FZ.Auth.Domain.Billing;
using FZ.Auth.Infrastructure;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Auth.Infrastructure.Repository.Billing;
using global::FZ.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface IOrderService
    {
        Task<(Order order, Invoice invoice)> CreateOrderAsync(int userId, int priceId, string provider, CancellationToken ct);
        Task<bool> MarkPaidAndActivateAsync(int orderId, string provider, string providerPaymentId, decimal paidAmount, CancellationToken ct);
        Task MarkFailedAsync(int orderId, string provider, string? reasonCode, CancellationToken ct);
        Task<Order?> GetOrderByID(int orderID, CancellationToken ct);
    }

    public class OrderService : IOrderService
    {
        private readonly AuthDbContext _db;
        private readonly IOrderRepository _orders;
        private readonly ISubscriptionService _subs;
        private readonly IUnitOfWork _uow;

        public OrderService(AuthDbContext db, IOrderRepository orders, ISubscriptionService subs , IUnitOfWork unitOfWork)
        {
            _db = db; _orders = orders; _subs = subs; _uow = unitOfWork;
        }

        public async Task<(Order order, Invoice invoice)> CreateOrderAsync(int userId, int priceId, string provider, CancellationToken ct)
        {
            return await _uow.ExecuteInTransactionAsync<(Order, Invoice)>(async t =>
            {
                var price = await _db.prices
                    .Include(p => p.plan)
                    .FirstOrDefaultAsync(p => p.priceID == priceId && p.isActive, t)
                    ?? throw new InvalidOperationException("Price not found");

                var now = DateTime.UtcNow;
                var txnRef = Guid.NewGuid().ToString("N"); // dùng làm providerSessionId / vnp_TxnRef

                var order = new Order
                {
                    userID = userId,
                    planID = price.planID,
                    priceID = price.priceID,
                    amount = price.amount,
                    currency = price.currency,
                    status = "pending",
                    provider = provider,
                    createdAt = now,
                    providerSessionId = txnRef,
                    expiresAt = now.AddMinutes(30)
                };
                _db.orders.Add(order);

                // Không cần SaveChanges để lấy orderID: EF tự set FK đúng khi save cuối
                var invoice = new Invoice
                {
                    userID = userId,
                    order = order,             // gán qua navigation, EF tự set orderID
                    subtotal = price.amount,
                    discount = 0,
                    tax = 0,
                    total = price.amount,
                    issuedAt = now
                };
                _db.invoices.Add(invoice);

                // KHÔNG SaveChanges ở đây – _uow sẽ gọi 1 lần trước Commit
                return (order, invoice);
            }, ct: ct);
        }


        public async Task<bool> MarkPaidAndActivateAsync(int orderId, string provider, string providerPaymentId, decimal paidAmount, CancellationToken ct)
        {
            return await _uow.ExecuteInTransactionAsync<bool>(async t =>
            {
                var order = await _db.orders.FirstOrDefaultAsync(o => o.orderID == orderId, t);
                if (order is null) return false;

                // Idempotent
                if (order.status is "paid" or "confirmed") return true;

                // Đối soát số tiền (tuỳ policy, có thể fail sớm)
                if (paidAmount != order.amount) return false;

                order.status = "paid";     // entity đang track, không cần _db.orders.Update(order);

                var invoice = await _db.invoices.FirstOrDefaultAsync(i => i.orderID == order.orderID, t);
                if (invoice is null) return false;

                var payment = await _db.payments.FirstOrDefaultAsync(p => p.invoiceID == invoice.invoiceID, t);
                if (payment is null)
                {
                    payment = new Payment
                    {
                        invoiceID = invoice.invoiceID,
                        provider = provider,
                        providerPaymentId = providerPaymentId,
                        status = "succeeded",
                        createdAt = DateTime.UtcNow,
                        paidAt = DateTime.UtcNow
                    };
                    _db.payments.Add(payment);
                }
                else
                {
                    payment.status = "succeeded";
                    payment.providerPaymentId = providerPaymentId;
                    payment.paidAt = DateTime.UtcNow;
                    // không cần Update()
                }

                await _subs.ActivateVipAsync(order.userID, order.priceID, autoRenew: false, t);

                // KHÔNG SaveChanges/Commit – _uow lo
                return true;
            }, ct: ct);
        }


        public async Task MarkFailedAsync(int orderId, string provider, string? reasonCode, CancellationToken ct)
        {
            await _uow.ExecuteInTransactionAsync<object?>(async t =>
            {
                var order = await _db.orders.FirstOrDefaultAsync(o => o.orderID == orderId, t);
                if (order is null) return null;

                order.status = "failed";

                var invoice = await _db.invoices.FirstOrDefaultAsync(i => i.orderID == orderId, t);
                if (invoice != null)
                {
                    var payment = await _db.payments.FirstOrDefaultAsync(p => p.invoiceID == invoice.invoiceID, t);
                    if (payment is null)
                    {
                        payment = new Payment
                        {
                            invoiceID = invoice.invoiceID,
                            provider = provider,
                            status = "failed",
                            failureReason = reasonCode,
                            createdAt = DateTime.UtcNow
                        };
                        _db.payments.Add(payment);
                    }
                    else
                    {
                        payment.status = "failed";
                        payment.failureReason = reasonCode;
                    }
                }
                return null;
            }, ct: ct);
        }


        public Task<Order?> GetOrderByID(int orderID, CancellationToken ct) => _orders.GetByIdAsync(orderID, ct);
    }
}


