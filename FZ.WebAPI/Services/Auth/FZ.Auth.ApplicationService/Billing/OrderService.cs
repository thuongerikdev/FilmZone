using FZ.Auth.Domain.Billing;
using FZ.Auth.Infrastructure;
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

        public OrderService(AuthDbContext db, IOrderRepository orders, ISubscriptionService subs)
        {
            _db = db; _orders = orders; _subs = subs;
        }

        public async Task<(Order, Invoice)> CreateOrderAsync(int userId, int priceId, string provider, CancellationToken ct)
        {
            var price = await _db.prices.Include(p => p.plan)
                .FirstOrDefaultAsync(p => p.priceID == priceId && p.isActive, ct)
                ?? throw new InvalidOperationException("Price not found");

            var now = DateTime.UtcNow;

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
                expiresAt = now.AddMinutes(30)
            };
            _db.orders.Add(order);
            await _db.SaveChangesAsync(ct);

            var invoice = new Invoice
            {
                userID = userId,
                orderID = order.orderID,
                subtotal = price.amount,
                discount = 0,
                tax = 0,
                total = price.amount,
                issuedAt = now
            };
            _db.invoices.Add(invoice);
            await _db.SaveChangesAsync(ct);

            return (order, invoice);
        }

        public async Task<bool> MarkPaidAndActivateAsync(int orderId, string provider, string providerPaymentId, decimal paidAmount, CancellationToken ct)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            var order = await _db.orders.FirstOrDefaultAsync(o => o.orderID == orderId, ct);
            if (order == null) return false;
            if (order.status is "paid" or "confirmed") { await tx.RollbackAsync(ct); return true; }

            // Optional: đối soát số tiền
            if (paidAmount != order.amount) { /* log cảnh báo hoặc reject tuỳ policy */ }

            order.status = "paid";
            _db.orders.Update(order);
            await _db.SaveChangesAsync(ct);

            var invoice = await _db.invoices.FirstAsync(i => i.orderID == order.orderID, ct);

            var payment = await _db.payments.FirstOrDefaultAsync(p => p.invoiceID == invoice.invoiceID, ct);
            if (payment == null)
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
                _db.payments.Update(payment);
            }
            await _db.SaveChangesAsync(ct);

            await _subs.ActivateVipAsync(order.userID, order.priceID, autoRenew: false, ct);

            await tx.CommitAsync(ct);
            return true;
        }

        public async Task MarkFailedAsync(int orderId, string provider, string? reasonCode, CancellationToken ct)
        {
            var order = await _db.orders.FirstOrDefaultAsync(o => o.orderID == orderId, ct);
            if (order == null) return;

            order.status = "failed";
            _db.orders.Update(order);

            var invoice = await _db.invoices.FirstOrDefaultAsync(i => i.orderID == orderId, ct);
            if (invoice != null)
            {
                var payment = await _db.payments.FirstOrDefaultAsync(p => p.invoiceID == invoice.invoiceID, ct);
                if (payment == null)
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
                    _db.payments.Update(payment);
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        public Task<Order?> GetOrderByID(int orderID, CancellationToken ct) => _orders.GetByIdAsync(orderID, ct);
    }
}


