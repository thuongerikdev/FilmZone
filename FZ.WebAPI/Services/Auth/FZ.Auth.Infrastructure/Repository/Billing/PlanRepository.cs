using FZ.Auth.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FZ.Auth.Infrastructure.Repository.Billing
{
    public interface IPlanRepository
    {
        Task<Plan> AddAsync(Plan plan, CancellationToken ct);
        Task<Plan> UpdateAsync(Plan plan, CancellationToken ct);
        Task<Plan> DeleteAsync(Plan plan, CancellationToken ct);
        Task<Plan?> GetByIDAsync(int planID, CancellationToken ct);
        Task<List<Plan>> GetAllAsync(CancellationToken ct);
    }

    public class PlanRepository : IPlanRepository
    {
        private readonly AuthDbContext _context;
        public PlanRepository(AuthDbContext context) => _context = context;

        public async Task<Plan> AddAsync(Plan plan, CancellationToken ct)
        {
            var entry = await _context.plans.AddAsync(plan, ct);
            return entry.Entity;
        }
        public async Task<Plan> UpdateAsync(Plan plan, CancellationToken ct)
        {
            var tracked = await _context.plans.FindAsync(new object[] { plan.planID }, ct);
            if (tracked is null)
                throw new KeyNotFoundException($"Plan with ID {plan.planID} not found.");

            _context.Entry(tracked).CurrentValues.SetValues(plan);
            return tracked;
        }

        public Task<Plan> DeleteAsync(Plan plan, CancellationToken ct)
        {
            _context.Attach(plan);
            var entry = _context.plans.Remove(plan);
            return Task.FromResult(entry.Entity);
        }

        public Task<Plan?> GetByIDAsync(int planID, CancellationToken ct)
            => _context.plans.AsNoTracking()
                             .FirstOrDefaultAsync(p => p.planID == planID, ct);

        public Task<List<Plan>> GetAllAsync(CancellationToken ct)
            => _context.plans.AsNoTracking().ToListAsync(ct);

       
    }
}
