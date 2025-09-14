using FZ.Movie.Domain.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Interactions
{
    public interface IEpisodeWatchProgressRepository
    {
        Task AddAsync(EpisodeWatchProgress? progress, CancellationToken ct);
        Task<EpisodeWatchProgress?> GetByIdAsync(int progressID, CancellationToken ct);
        Task<EpisodeWatchProgress?> GetTrackedAsync(int progressID, CancellationToken ct);
        Task<bool> ExistsAsync(int progressID, CancellationToken ct);
        Task UpdateAsync(EpisodeWatchProgress progress, CancellationToken ct);
        Task<bool> PatchAsync(int progressID, Action<EpisodeWatchProgress> apply, CancellationToken ct);
        Task RemoveAsync(int progressID);
        Task<int> HardDeleteAsync(int progressID, CancellationToken ct);
        Task<List<EpisodeWatchProgress>> GetAllByUserIdAsync(int userId, CancellationToken ct);
        Task<List<EpisodeWatchProgress>> GetAllByEposodeIDAsync(int movieId, CancellationToken ct);
        Task<List<EpisodeWatchProgress>> GetAllEpisodeAsync( CancellationToken ct);
    }
    public sealed class EpisodeWatchingProgressRepository : IEpisodeWatchProgressRepository
    {
        private readonly MovieDbContext _context;
        public EpisodeWatchingProgressRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(EpisodeWatchProgress progress, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(progress);
            return _context.EpisodeWatchProgresses.AddAsync(progress, ct).AsTask();
        }
        public Task<EpisodeWatchProgress?> GetByIdAsync(int progressID, CancellationToken ct)
            => _context.EpisodeWatchProgresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.episodeWatchProgressID == progressID, ct);
        public Task<EpisodeWatchProgress?> GetTrackedAsync(int progressID, CancellationToken ct)
            => _context.EpisodeWatchProgresses.FirstOrDefaultAsync(x => x.episodeWatchProgressID == progressID, ct);
        public Task<bool> ExistsAsync(int progressID, CancellationToken ct)
            => _context.EpisodeWatchProgresses.AsNoTracking().AnyAsync(x => x.episodeWatchProgressID == progressID, ct);
        public Task UpdateAsync(EpisodeWatchProgress progress, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(progress);
            _context.EpisodeWatchProgresses.Update(progress);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int progressID, Action<EpisodeWatchProgress> apply, CancellationToken ct)
        {
            var progress = await GetTrackedAsync(progressID, ct);
            if (progress is null) return false;
            apply(progress);
            return true;
        }
        public  Task RemoveAsync(int progressID)
        {
           var stub = new EpisodeWatchProgress { episodeWatchProgressID = progressID };
              _context.Entry(stub).State = EntityState.Deleted;
                return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int progressID, CancellationToken ct)
            => _context.EpisodeWatchProgresses
                .Where(c => c.episodeWatchProgressID == progressID)
                .ExecuteDeleteAsync(ct);
        public Task<List<EpisodeWatchProgress>> GetAllByUserIdAsync(int userId, CancellationToken ct)
            => _context.EpisodeWatchProgresses
                .AsNoTracking()
                .Where(x => x.userID == userId)
                .ToListAsync(ct);
        public Task<List<EpisodeWatchProgress>> GetAllByEposodeIDAsync(int movieId, CancellationToken ct)
            => _context.EpisodeWatchProgresses
                .AsNoTracking()
                .Where(x => x.episodeID == movieId)
                .ToListAsync(ct);
        public Task<List<EpisodeWatchProgress>> GetAllEpisodeAsync(CancellationToken ct)
            => _context.EpisodeWatchProgresses
                .AsNoTracking()
                .ToListAsync(ct);
    }
}
