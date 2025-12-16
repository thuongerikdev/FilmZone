using FZ.Auth.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync (Order entity, CancellationToken ct);
        Task<Order> UpdateAsync (Order entity, CancellationToken ct); 
        Task<Order> DeleteAsync (Order entity, CancellationToken ct);
        Task<Order> GetByIdAsync (int orderID, CancellationToken ct);
        Task<List<Order>> GetAllAsync (CancellationToken ct);
        Task<List<Order>> GetOrdersByUserID (int userID, CancellationToken ct);
        Task<List<Order>> GetOrdersByStatus (string status, CancellationToken ct);

    }
    public class OrderRepository : IOrderRepository
    {
        private readonly AuthDbContext _context;
        public OrderRepository (AuthDbContext context)
        {
            _context = context;
        }
        public async Task<Order> AddAsync (Order entity, CancellationToken ct)
        {
            var result = await _context.orders.AddAsync(entity, ct);
            return result.Entity;

        }
        public async Task<Order> UpdateAsync (Order entity, CancellationToken ct)
        {
            var existingOrder = await _context.orders.FindAsync(new object[] { entity.orderID }, ct);
            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with ID {entity.orderID} not found.");
            }
            _context.Entry(existingOrder).CurrentValues.SetValues(entity);
            return existingOrder;
        }
        public  Task<Order> DeleteAsync (Order entity, CancellationToken ct)
        {
            _context.Attach(entity);
            var result = _context.orders.Remove(entity);
            return Task.FromResult(result.Entity);

        }
        public async Task<Order> GetByIdAsync (int orderID, CancellationToken ct)
        {
            return await _context.orders.FirstOrDefaultAsync(o => o.orderID == orderID, ct);

        }
        public async Task<List<Order>> GetOrdersByUserID (int userID, CancellationToken ct)
        {
            return await _context.orders.Where(o => o.userID == userID).ToListAsync(ct);
        }
        public async Task<List<Order>> GetOrdersByStatus (string status, CancellationToken ct)
        {
            return await _context.orders.Where(o => o.status == status).ToListAsync(ct);
        }
        public async Task<List<Order>> GetAllAsync (CancellationToken ct)
        {
            return await _context.orders.ToListAsync(ct);
        }


    }
}
