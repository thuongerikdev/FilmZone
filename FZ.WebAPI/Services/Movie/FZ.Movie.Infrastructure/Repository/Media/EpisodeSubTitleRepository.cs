using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IEpisodeSubTitleRepository
    {
        // CREATE
        Task AddAsync(Domain.Media.EpisodeSubTitle episodeSubTitle, CancellationToken ct);
        // READ
        Task<Domain.Media.EpisodeSubTitle?> GetByIdAsync(int episodeSubTitleID, CancellationToken ct);                  // no-tracking (để đọc)
        Task<Domain.Media.EpisodeSubTitle?> GetTrackedAsync(int episodeSubTitleID, CancellationToken ct);               // tracked (để update)
        Task<bool> ExistsAsync(int episodeSubTitleID, CancellationToken ct);
        // UPDATE
        Task UpdateAsync(Domain.Media.EpisodeSubTitle episodeSubTitle);                                 // Update entity đã tracked
        Task PatchAsync(int episodeSubTitleID, Action<Domain.Media.EpisodeSubTitle> apply, CancellationToken ct); // tải-tracked rồi áp thay đổi
        // DELETE
        Task RemoveAsync(int episodeSubTitleID);                                 // mark Deleted -> UoW sẽ commit
        Task<int> HardDeleteAsync(int episodeSubTitleID, CancellationToken ct);  // bulk delete ngay trên DB (EF Core 7+)
        Task<List<Domain.Media.EpisodeSubTitle>> GetAllEpisodeSubTitleAsync(CancellationToken ct);
        Task<List<Domain.Media.EpisodeSubTitle>> GetByEpisodeSourceIDAsync(int episodeSourceID, CancellationToken ct);
    }
    public class EpisodeSubTitleRepository : IEpisodeSubTitleRepository
    {
        private readonly MovieDbContext _context;
        public EpisodeSubTitleRepository(MovieDbContext context)
        {
            _context = context;
        }
        // ------------- CREATE -------------
        public Task AddAsync(Domain.Media.EpisodeSubTitle episodeSubTitle, CancellationToken ct)
            => _context.EpisodeSubTitles.AddAsync(episodeSubTitle, ct).AsTask();
        // ------------- READ -------------
        // Đọc nhẹ, không tracking
        public Task<Domain.Media.EpisodeSubTitle?> GetByIdAsync(int episodeSubTitleID, CancellationToken ct)
            => _context.EpisodeSubTitles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.episodeSubTitleID == episodeSubTitleID, ct);
        // Dùng cho cập nhật: entity được track
        public Task<Domain.Media.EpisodeSubTitle?> GetTrackedAsync(int episodeSubTitleID, CancellationToken ct)
            => _context.EpisodeSubTitles
                .FirstOrDefaultAsync(x => x.episodeSubTitleID == episodeSubTitleID, ct);
        public Task<bool> ExistsAsync(int episodeSubTitleID, CancellationToken ct)
            => _context.EpisodeSubTitles.AsNoTracking().AnyAsync(x => x.episodeSubTitleID == episodeSubTitleID, ct);

        // ------------- UPDATE -------------
        public Task UpdateAsync(Domain.Media.EpisodeSubTitle episodeSubTitle)
        {
            _context.EpisodeSubTitles.Update(episodeSubTitle);
            return Task.CompletedTask;
        }
        public async Task PatchAsync(int episodeSubTitleID, Action<Domain.Media.EpisodeSubTitle> apply, CancellationToken ct)
        {
            var entity = await GetTrackedAsync(episodeSubTitleID, ct);
            if (entity != null)
            {
                apply(entity);
            }
        }
        // ------------- DELETE -------------
        public Task RemoveAsync(int episodeSubTitleID)
        {
            var entity = new Domain.Media.EpisodeSubTitle { episodeSubTitleID = episodeSubTitleID };
            _context.EpisodeSubTitles.Remove(entity);
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int episodeSubTitleID, CancellationToken ct)
            => _context.EpisodeSubTitles
                .Where(x => x.episodeSubTitleID == episodeSubTitleID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Media.EpisodeSubTitle>> GetAllEpisodeSubTitleAsync(CancellationToken ct)
            => _context.EpisodeSubTitles
                .AsNoTracking()
                .ToListAsync(ct);
        public Task<List<Domain.Media.EpisodeSubTitle>> GetByEpisodeSourceIDAsync(int episodeSourceID, CancellationToken ct)
            => _context.EpisodeSubTitles
                .AsNoTracking()
                .Where(x => x.episodeSourceID == episodeSourceID)
                .ToListAsync(ct);

    }
}
