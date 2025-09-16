using FZ.Auth.Domain.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync (Payment payment, CancellationToken ct);
        Task<Payment> UpdateAsync (Payment payment, CancellationToken ct);
        Task<Payment> GetByIdAsync (int paymentID, CancellationToken ct);
        Task <Payment> DeleteAsync (Payment payment, CancellationToken ct);
       
    }
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AuthDbContext _context;
        public PaymentRepository(AuthDbContext context)
        {
            _context = context;
        }
        public async Task<Payment> AddAsync(Payment payment, CancellationToken ct)
        {
            var entity = await _context.payments.AddAsync(payment, ct);
            return entity.Entity;
        }
        public  Task<Payment> DeleteAsync(Payment payment, CancellationToken ct)
        {
             _context.Attach(payment);
            var result =  _context.payments.Remove(payment);
            return Task.FromResult(result.Entity);


        }
        public async Task<Payment> GetByIdAsync(int paymentID, CancellationToken ct)
        {
            var payment = await _context.payments.FindAsync(new object[] { paymentID }, ct);
            return payment;
        }
     

        public async Task<Payment> UpdateAsync(Payment payment, CancellationToken ct)
        {
            var entity = await _context.payments.FindAsync(new object[] { payment.paymentID }, ct);
            if (entity == null)
            {
                throw new Exception("Payment not found");
            }
            _context.Entry(entity).CurrentValues.SetValues(payment);
            return entity;

        }
    }
}
