using FZ.Movie.Domain.Media;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IMovieSourceRepository
    {
        // CREATE
        Task AddAsync(Domain.Media.MovieSource movieSource, CancellationToken ct);
        // READ
        Task<Domain.Media.MovieSource?> GetByIdAsync(int movieSourceID, CancellationToken ct);                  // no-tracking (để đọc)
        Task<Domain.Media.MovieSource?> GetTrackedAsync(int movieSourceID, CancellationToken ct);               // tracked (để update)
        Task<bool> ExistsAsync(int movieSourceID, CancellationToken ct);
        Task<int> CountAsync(CancellationToken ct,
            int? movieID = null);
        // UPDATE
        Task UpdateAsync(Domain.Media.MovieSource movieSource);                                 // Update entity đã tracked
        Task PatchAsync(int movieSourceID, Action<Domain.Media.MovieSource> apply, CancellationToken ct); // tải-tracked rồi áp thay đổi
        // DELETE
        Task RemoveAsync(int movieSourceID);                                 // mark Deleted -> UoW sẽ commit
        Task<int> HardDeleteAsync(int movieSourceID, CancellationToken ct);  // bulk delete ngay trên DB (EF Core 7+)
        Task<List<Domain.Media.MovieSource>> GetAllMovieSourceAsync(CancellationToken ct);
        Task<MovieSource> GetBySourceID (string sourceID, CancellationToken ct);
        Task<List<MovieSource>> GetByMovieID (int movieID, CancellationToken ct);
        Task<MovieSource?> GetByCompositeKeyAsync(
        int movieID, string sourceType, string sourceID, string? language, string? quality, CancellationToken ct);

        Task Add(MovieSource entity, CancellationToken ct);
        Task UpdateAsync(MovieSource entity, CancellationToken ct);
    }
    public sealed class MovieSourceRepository : IMovieSourceRepository
    {
        private readonly MovieDbContext _context;
        public MovieSourceRepository(MovieDbContext context)
        {
            _context = context;
        }
        // ------------- CREATE -------------
        public Task AddAsync(Domain.Media.MovieSource movieSource, CancellationToken ct)
            => _context.MovieSources.AddAsync(movieSource, ct).AsTask();
        // ------------- READ -------------
        // Đọc nhẹ, không tracking
        public Task<Domain.Media.MovieSource?> GetByIdAsync(int movieSourceID, CancellationToken ct)
            => _context.MovieSources
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieSourceID == movieSourceID, ct);
        // Dùng cho cập nhật: entity được track
        public Task<Domain.Media.MovieSource?> GetTrackedAsync(int movieSourceID, CancellationToken ct)
            => _context.MovieSources
                .FirstOrDefaultAsync(x => x.movieSourceID == movieSourceID, ct);
        public Task<bool> ExistsAsync(int movieSourceID, CancellationToken ct)
            => _context.MovieSources.AsNoTracking().AnyAsync(x => x.movieSourceID == movieSourceID, ct);
        public Task<int> CountAsync(CancellationToken ct,
            int? movieID = null)
        {
            var query = _context.MovieSources.AsNoTracking().AsQueryable();
            if (movieID.HasValue)
            {
                query = query.Where(x => x.movieID == movieID.Value);
            }
            return query.CountAsync(ct);
        }
        // ------------- UPDATE -------------
        public Task UpdateAsync(Domain.Media.MovieSource movieSource)
        {
            ArgumentNullException.ThrowIfNull(movieSource);
            // Nếu đã attach thì Update() sẽ mark Modified; nếu detached, ta attach rồi mark Modified
            _context.MovieSources.Update(movieSource);
            return Task.CompletedTask;
        }
        public async Task PatchAsync(int movieSourceID, Action<Domain.Media.MovieSource> apply, CancellationToken ct)
        {
            var movieSource = await GetTrackedAsync(movieSourceID, ct);
            if (movieSource is null) throw new KeyNotFoundException($"Không tìm thấy MovieSource với ID {movieSourceID}");
            apply(movieSource);
        }
        // ------------- DELETE -------------
        public Task RemoveAsync(int movieSourceID)
        {
            var stub = new Domain.Media.MovieSource { movieSourceID = movieSourceID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int movieSourceID, CancellationToken ct)
            => _context.MovieSources.Where(c => c.movieSourceID == movieSourceID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Media.MovieSource>> GetAllMovieSourceAsync(CancellationToken ct)
            => _context.MovieSources.AsNoTracking()
                .ToListAsync(ct);

        public Task<MovieSource> GetBySourceID(string sourceID, CancellationToken ct)
            => _context.MovieSources.AsNoTracking()
                .FirstOrDefaultAsync(x => x.sourceID == sourceID, ct);
        public Task<List<MovieSource>> GetByMovieID(int movieID, CancellationToken ct)
            => _context.MovieSources.AsNoTracking()
                .Where(x => x.movieID == movieID)
                .ToListAsync(ct);

        public async Task<MovieSource?> GetByCompositeKeyAsync(
        int movieID, string sourceType, string sourceID, string? language, string? quality, CancellationToken ct)
        {
            var q = _context.Set<MovieSource>().AsQueryable();

            q = q.Where(x =>
                x.movieID == movieID &&
                x.sourceType == sourceType &&
                x.sourceID == sourceID);

            if (language is null) q = q.Where(x => x.language == null);
            else q = q.Where(x => x.language == language);

            if (quality is null) q = q.Where(x => x.quality == null);
            else q = q.Where(x => x.quality == quality);

            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task Add(MovieSource entity, CancellationToken ct)
        {
            await _context.Set<MovieSource>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(MovieSource entity, CancellationToken ct)
        {
            _context.Set<MovieSource>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
           
}
