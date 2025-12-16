using FZ.Auth.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface IInvoiceRepository
    {
        Task<Invoice> AddAsync(Invoice entity, CancellationToken ct);
        Task<Invoice> UpdateAsync(Invoice entity, CancellationToken ct);
        Task<Invoice?> GetByIdAsync(int id, CancellationToken ct);
        Task<Invoice> DeleteAsync(Invoice entity, CancellationToken ct);
        Task<List<Invoice>> GetAllAsync(CancellationToken ct);
        Task<List<Invoice>> GetInvoicesByUserID(int userID, CancellationToken ct);

    }
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AuthDbContext _context;
        public InvoiceRepository(AuthDbContext context)
        {
            _context = context;
        }
        public async Task<Invoice> AddAsync(Invoice entity, CancellationToken ct)
        {
            var result = await _context.invoices.AddAsync(entity, ct);
            return result.Entity;
        }
        public Task<Invoice> DeleteAsync(Invoice entity, CancellationToken ct)
        {
            _context.Attach(entity);
            var entry = _context.invoices.Remove(entity);
            return Task.FromResult(entry.Entity);
        }
        public async Task<Invoice?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _context.invoices.FindAsync(new object?[] { id }, ct);
        }
        public async Task<List<Invoice>> GetInvoicesByUserID(int userID, CancellationToken ct)
        {
            return await _context.invoices.Where(i => i.userID == userID).ToListAsync(ct);
        }
        public async Task<Invoice> UpdateAsync(Invoice entity, CancellationToken ct)
        {
            var existingInvoice = await _context.invoices.FindAsync(new object?[] { entity.invoiceID }, ct);
            if (existingInvoice == null)
            {
                throw new KeyNotFoundException($"Invoice with ID {entity.invoiceID} not found");

            }
            _context.Entry(existingInvoice).CurrentValues.SetValues(entity);
            return existingInvoice;
        }
        public Task<List<Invoice>> GetAllAsync(CancellationToken ct)
        {
            return _context.invoices.ToListAsync(ct);
        }
    }
}

