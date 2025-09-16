using FZ.Auth.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface IUserSubscriptionRepository
    {
        Task<UserSubscription> AddAsync(UserSubscription entity, CancellationToken ct);
        Task<UserSubscription> UpdateAsync(UserSubscription entity, CancellationToken ct);
        Task<UserSubscription?> GetByIdAsync(int id, CancellationToken ct);
        Task<UserSubscription?> GetByUserIdAsync(int userId, CancellationToken ct);
        Task<UserSubscription> DeleteAsync(UserSubscription entity, CancellationToken ct);
        Task<List<UserSubscription>> GetAllAsync(CancellationToken ct);
    }

    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly AuthDbContext _context;

        public UserSubscriptionRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<UserSubscription> AddAsync(UserSubscription entity, CancellationToken ct)
        {
            var entry = await _context.userSubscriptions.AddAsync(entity, ct);
            return entry.Entity;
        }

        public async Task<UserSubscription> UpdateAsync(UserSubscription entity, CancellationToken ct)
        {

            var tracked = await _context.userSubscriptions.FindAsync(new object[] { entity.subscriptionID }, ct);
            if (tracked is null)
                throw new KeyNotFoundException($"UserSubscription with ID {entity.subscriptionID} not found.");

            _context.Entry(tracked).CurrentValues.SetValues(entity);
            return tracked;
        }

        public Task<UserSubscription> DeleteAsync(UserSubscription entity, CancellationToken ct)
        {
            _context.Attach(entity);
            var entry = _context.userSubscriptions.Remove(entity);
            return Task.FromResult(entry.Entity);
        }

        public Task<List<UserSubscription>> GetAllAsync(CancellationToken ct)
            => _context.userSubscriptions.AsNoTracking().ToListAsync(ct);

        public Task<UserSubscription?> GetByIdAsync(int id, CancellationToken ct)
            => _context.userSubscriptions.AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.subscriptionID == id, ct);

        public Task<UserSubscription?> GetByUserIdAsync(int userId, CancellationToken ct)
            => _context.userSubscriptions.AsNoTracking()
                                         .FirstOrDefaultAsync(us => us.userID == userId, ct);
    }
}
