using FZ.Movie.Domain.Media;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IEpisodeSourceRepository
    {
        // CREATE
        Task AddAsync(Domain.Media.EpisodeSource episodeSource, CancellationToken ct);
        // READ
        Task<Domain.Media.EpisodeSource?> GetByIdAsync(int episodeSourceID, CancellationToken ct);                  // no-tracking (để đọc)
        Task<Domain.Media.EpisodeSource?> GetTrackedAsync(int episodeSourceID, CancellationToken ct);               // tracked (để update)
        Task<bool> ExistsAsync(int episodeSourceID, CancellationToken ct);
     
        Task<int> CountAsync(CancellationToken ct,
            int? episodeID = null);
        // UPDATE
        Task UpdateAsync(Domain.Media.EpisodeSource episodeSource);                                 // Update entity đã tracked
        Task PatchAsync(int episodeSourceID, Action<Domain.Media.EpisodeSource> apply, CancellationToken ct); // tải-tracked rồi áp thay đổi
        // DELETE
        Task RemoveAsync(int episodeSourceID);                                 // mark Deleted -> UoW sẽ commit
        Task<int> HardDeleteAsync(int episodeSourceID, CancellationToken ct);  // bulk delete ngay trên DB (EF Core 7+)
        Task<List<Domain.Media.EpisodeSource>> GetAllEpisodeSourceAsync(CancellationToken ct);
        Task<EpisodeSource?> GetBySourceIDAsync(string sourceID, CancellationToken ct);
        Task<List<EpisodeSource>> GetAllByEpisodeIDAsync(int episodeID, CancellationToken ct);
        Task<EpisodeSource?> GetByCompositeKeyAsync(
        int episodeID, string sourceType, string sourceID, string? language, string? quality, CancellationToken ct);

        Task Add(EpisodeSource entity, CancellationToken ct);
        Task UpdateAsync(EpisodeSource entity, CancellationToken ct);
    }
    public sealed class EpisodeSourceRepository : IEpisodeSourceRepository
    {
        private readonly MovieDbContext _context;
        public EpisodeSourceRepository(MovieDbContext context)
        {
            _context = context;
        }
        // ------------- CREATE -------------
        public Task AddAsync(Domain.Media.EpisodeSource episodeSource, CancellationToken ct)
            => _context.EpisodeSources.AddAsync(episodeSource, ct).AsTask();
        // ------------- READ -------------
        // Đọc nhẹ, không tracking
        public Task<Domain.Media.EpisodeSource?> GetByIdAsync(int episodeSourceID, CancellationToken ct)
            => _context.EpisodeSources
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.episodeSourceID == episodeSourceID, ct);
        // Dùng cho cập nhật: entity được track
        public Task<Domain.Media.EpisodeSource?> GetTrackedAsync(int episodeSourceID, CancellationToken ct)
            => _context.EpisodeSources
                .FirstOrDefaultAsync(x => x.episodeSourceID == episodeSourceID, ct);
        public Task<bool> ExistsAsync(int episodeSourceID, CancellationToken ct)
            => _context.EpisodeSources.AsNoTracking().AnyAsync(x => x.episodeSourceID == episodeSourceID, ct);

        public Task<int> CountAsync(CancellationToken ct,
            int? episodeID = null)
        {
            var query = _context.EpisodeSources.AsNoTracking().AsQueryable();
            if (episodeID.HasValue)
            {
                query = query.Where(x => x.episodeID == episodeID.Value);
            }
            return query.CountAsync(ct);
        }
        // ------------- UPDATE
        public Task UpdateAsync(Domain.Media.EpisodeSource episodeSource)
        {
            ArgumentNullException.ThrowIfNull(episodeSource);
            // Nếu đã attach thì Update() sẽ mark Modified; nếu detached, ta attach rồi mark Modified
            var entry = _context.Entry(episodeSource);
            if (entry.State == EntityState.Detached)
            {
                _context.Attach(episodeSource);
                entry.State = EntityState.Modified;
            }
            else
            {
                _context.EpisodeSources.Update(episodeSource);
            }
            return Task.CompletedTask;
        }
        public async Task PatchAsync(int episodeSourceID, Action<Domain.Media.EpisodeSource> apply, CancellationToken ct)
        {
            var episodeSource = await GetTrackedAsync(episodeSourceID, ct);
            if (episodeSource is null) return;
            apply(episodeSource);
        }
        // ------------- DELETE
        public  Task RemoveAsync(int episodeSourceID)
        {
            var stub = new Domain.Media.EpisodeSource { episodeSourceID = episodeSourceID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;


        }
        public Task<int> HardDeleteAsync(int episodeSourceID, CancellationToken ct)
            => _context.EpisodeSources.Where(c => c.episodeSourceID == episodeSourceID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Media.EpisodeSource>> GetAllEpisodeSourceAsync(CancellationToken ct)
            => _context.EpisodeSources.AsNoTracking().ToListAsync(ct);
        public Task<EpisodeSource?> GetBySourceIDAsync(string sourceID, CancellationToken ct)
            => _context.EpisodeSources.AsNoTracking()
                .FirstOrDefaultAsync(x => x.sourceID == sourceID, ct);
        public Task<List<EpisodeSource>> GetAllByEpisodeIDAsync(int episodeID, CancellationToken ct)
            => _context.EpisodeSources.AsNoTracking()
                .Where(x => x.episodeID == episodeID)
                .ToListAsync(ct);
        public async Task<EpisodeSource?> GetByCompositeKeyAsync(
       int episodeID, string sourceType, string sourceID, string? language, string? quality, CancellationToken ct)
        {
            var q = _context.Set<EpisodeSource>().AsQueryable();

            q = q.Where(x =>
                x.episodeID == episodeID &&
                x.sourceType == sourceType &&
                x.sourceID == sourceID);

            // ràng buộc language/quality theo đúng giá trị (kể cả null)
            if (language is null) q = q.Where(x => x.language == null);
            else q = q.Where(x => x.language == language);

            if (quality is null) q = q.Where(x => x.quality == null);
            else q = q.Where(x => x.quality == quality);

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task Add(EpisodeSource entity, CancellationToken ct)
        {
            await _context.Set<EpisodeSource>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(EpisodeSource entity, CancellationToken ct)
        {
            _context.Set<EpisodeSource>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
