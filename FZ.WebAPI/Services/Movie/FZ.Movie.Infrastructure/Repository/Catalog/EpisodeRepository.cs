using FZ.Movie.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Catalog
{
    public interface IEpisodeRepository
    {
        // CREATE
        Task AddEpisodeAsync(Episode episode, CancellationToken ct);

        // READ
        Task<Episode?> GetByIdAsync(int episodeID, CancellationToken ct);
        Task<Episode?> GetTrackedAsync(int episodeID, CancellationToken ct);
        Task<bool> ExistsAsync(int episodeID, CancellationToken ct);

        // UPDATE
        Task UpdateAsync(Episode episode, CancellationToken ct);

        /// <summary>Áp dụng thay đổi một phần; trả về false nếu không tìm thấy.</summary>
        Task<bool> PatchAsync(int episodeID, Action<Episode> apply, CancellationToken ct);

        // DELETE (soft/local remove khỏi DbContext)
        Task RemoveAsync(int episodeID);

        // DELETE cứng (SQL)
        Task<int> HardDeleteAsync(int episodeID, CancellationToken ct);
        Task<List<Episode>> GetEpisodesByMovieIdAsync(int movieID, CancellationToken ct);
        Task<List<Episode>> GetAllEpisodeAsync(CancellationToken ct);
        Task<Episode?> GetByTitleAsync(string title, CancellationToken ct);
    }

    public sealed class EpisodeRepository : IEpisodeRepository
    {
        private readonly MovieDbContext _context;
        public EpisodeRepository(MovieDbContext context) => _context = context;

        // CREATE
        public Task AddEpisodeAsync(Episode episode, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(episode);
            return _context.Episodes.AddAsync(episode, ct).AsTask();
        }

        // READ (no-tracking cho đọc nhẹ)
        public Task<Episode?> GetByIdAsync(int episodeID, CancellationToken ct)
            => _context.Episodes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.episodeID == episodeID, ct);

        // READ tracked (phục vụ cập nhật)
        public Task<Episode?> GetTrackedAsync(int episodeID, CancellationToken ct)
            => _context.Episodes.FirstOrDefaultAsync(x => x.episodeID == episodeID, ct);

        public Task<bool> ExistsAsync(int episodeID, CancellationToken ct)
            => _context.Episodes.AsNoTracking().AnyAsync(x => x.episodeID == episodeID, ct);

        // UPDATE
        public Task UpdateAsync(Episode episode, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(episode);
            // Nếu đã attach thì Update() sẽ mark Modified; nếu detached, ta attach rồi mark Modified
            var entry = _context.Entry(episode);
            if (entry.State == EntityState.Detached)
            {
                _context.Attach(episode);
                entry.State = EntityState.Modified;
            }
            else
            {
                _context.Episodes.Update(episode);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> PatchAsync(int episodeID, Action<Episode> apply, CancellationToken ct)
        {
            var episode = await GetTrackedAsync(episodeID, ct);
            if (episode is null) return false;
            apply(episode);
            return true;
        }

        // DELETE (remove entity đã biết key, không cần load)
        public Task RemoveAsync(int episodeID)
        {
            var stub = new Episode { episodeID = episodeID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;

        }

        // DELETE cứng
        public Task<int> HardDeleteAsync(int episodeID, CancellationToken ct)
        => _context.Episodes.Where(c => c.episodeID == episodeID)
                .ExecuteDeleteAsync(ct);

        public Task<List<Episode>> GetEpisodesByMovieIdAsync(int movieID, CancellationToken ct)
            => _context.Episodes.AsNoTracking()
                .Where(e => e.movieID == movieID)
                .ToListAsync(ct);
        public Task<List<Episode>> GetAllEpisodeAsync(CancellationToken ct)
            => _context.Episodes.AsNoTracking()
                .ToListAsync(ct);

        public Task<Episode?> GetByTitleAsync(string title, CancellationToken ct)
            => _context.Episodes.AsNoTracking()
                .FirstOrDefaultAsync(e => e.title == title, ct);
    }

}
