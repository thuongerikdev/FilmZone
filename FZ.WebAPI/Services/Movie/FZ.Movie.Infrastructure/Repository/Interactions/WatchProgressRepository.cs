using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Interactions
{
    public interface IWatchProgressRepository
    {
        Task AddAsync(Domain.Interactions.WatchProgress watchProgress, CancellationToken ct);
        Task<Domain.Interactions.WatchProgress?> GetByIdAsync(int watchProgressID, CancellationToken ct);
        Task<Domain.Interactions.WatchProgress?> GetTrackedAsync(int watchProgressID, CancellationToken ct);
        Task<bool> ExistsAsync(int watchProgressID, CancellationToken ct);
        Task UpdateAsync(Domain.Interactions.WatchProgress watchProgress, CancellationToken ct);
        //Task<bool> PatchAsync(int watchProgressID, Action<Domain.Interactions.WatchProgress?> apply, CancellationToken ct);
        Task RemoveAsync(int watchProgressID);
        Task<int> HardDeleteAsync(int watchProgressID, CancellationToken ct);
        Task
            <List<Domain.Interactions.WatchProgress>> GetAllByUserIdAsync(int userId, CancellationToken ct);
        Task<List<Domain.Interactions.WatchProgress>> GetAllWatchProgressAsync(CancellationToken ct);
        Task<List<Domain.Interactions.WatchProgress>> GetAllByMovieIDAsync(int movieId, CancellationToken ct);
    }
    public sealed class WatchProgressRepository : IWatchProgressRepository
    {
        private readonly MovieDbContext _context;
        public WatchProgressRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.Interactions.WatchProgress watchProgress, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(watchProgress);
            return _context.WatchProgresses.AddAsync(watchProgress, ct).AsTask();
        }
        public Task<Domain.Interactions.WatchProgress?> GetByIdAsync(int watchProgressID, CancellationToken ct)
            => _context.WatchProgresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.watchProgressID == watchProgressID, ct);
        public Task<Domain.Interactions.WatchProgress?> GetTrackedAsync(int watchProgressID, CancellationToken ct)
            => _context.WatchProgresses.FirstOrDefaultAsync(x => x.watchProgressID == watchProgressID, ct);
        public Task<bool> ExistsAsync(int watchProgressID, CancellationToken ct)
            => _context.WatchProgresses.AsNoTracking().AnyAsync(x => x.watchProgressID == watchProgressID, ct);
        public Task UpdateAsync(Domain.Interactions.WatchProgress watchProgress, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(watchProgress);
            _context.WatchProgresses.Update(watchProgress);
            return Task.CompletedTask;
        }
        //public async Task<bool> PatchAsync(int watchProgressID, Action<Domain.Interactions.WatchProgress?> apply, CancellationToken ct)
        //{
        //    var watchProgress = await GetTrackedAsync(watchProgressID, ct);
        //    if (watchProgress is null) return false;
        //    apply(watchProgress!);
        //    return true;
        //}
        public  Task RemoveAsync(int watchProgressID)
        {
            var stub = new Domain.Interactions.WatchProgress { watchProgressID = watchProgressID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int watchProgressID, CancellationToken ct)
            => _context.WatchProgresses.Where(c => c.watchProgressID == watchProgressID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Interactions.WatchProgress>> GetAllByUserIdAsync(int userId, CancellationToken ct)
            => _context.WatchProgresses.AsNoTracking()
                .Where(wp => wp.userID == userId)
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.WatchProgress>> GetAllWatchProgressAsync(CancellationToken ct)
            => _context.WatchProgresses.AsNoTracking()
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.WatchProgress>> GetAllByMovieIDAsync(int movieId, CancellationToken ct)
            => _context.WatchProgresses.AsNoTracking()
                .Where(wp => wp.movieID == movieId)
                .ToListAsync(ct);
    }
}
