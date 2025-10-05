using FZ.Auth.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface  IPriceRepository
    {
        Task<Price> AddAsync (Price entity, CancellationToken ct);
        Task<Price> UpdateAsync (Price entity, CancellationToken ct);
        Task<Price> DeleteAsync (Price entity, CancellationToken ct);
        Task<Price?> GetByIdAsync (int priceID, CancellationToken ct);
        Task<List<Price>> GetAllAsync (CancellationToken ct);
    }
    public class PriceRepository : IPriceRepository
    {
        private readonly AuthDbContext _context;
        public PriceRepository(AuthDbContext context)
        {
            _context = context;
        }
        public async Task<Price> AddAsync(Price entity, CancellationToken ct)
        {
            var result = await _context.prices.AddAsync(entity, ct);
            return result.Entity;
        }
        public async  Task<Price> UpdateAsync(Price entity, CancellationToken ct)
        {
            var trackedEntity = await _context.prices.FindAsync(new object[] { entity.priceID }, ct);
            if (trackedEntity is null)
                throw new KeyNotFoundException($"Plan with ID {entity.priceID} not found.");

            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            return trackedEntity;

        }
        public async Task<Price> DeleteAsync(Price entity, CancellationToken ct)
        {
            _context.Attach(entity);

            // Ví dụ các bảng con tham chiếu priceID:
            await _context.orders
                .Where(o => o.priceID == entity.priceID)
                .ExecuteDeleteAsync(ct);

            // Nếu có OrderDetails/Payments… cùng tham chiếu priceID, xoá chúng trước:
            // await _context.orderDetails.Where(d => d.priceID == entity.priceID).ExecuteDeleteAsync(ct);
            // await _context.payments.Where(p => p.priceID == entity.priceID).ExecuteDeleteAsync(ct);

            var entry = _context.prices.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return entry.Entity;
        }
        public async Task<Price?> GetByIdAsync(int priceID, CancellationToken ct)
        {
            return await _context.prices.AsNoTracking().FirstOrDefaultAsync(p => p.priceID == priceID, ct);
        }
        public async Task<List<Price>> GetAllAsync(CancellationToken ct)
        {
            return await _context.prices.AsNoTracking().ToListAsync(ct);
        }

    }
}
