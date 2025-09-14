using FZ.Movie.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Catalog
{
    public interface IMovieRepository
    {
        // CREATE
        Task AddAsync(Movies movie, CancellationToken ct);

        // READ
        Task<Movies?> GetByIdAsync(int movieID, CancellationToken ct);                  // no-tracking (để đọc)
        Task<Movies?> GetTrackedAsync(int movieID, CancellationToken ct);               // tracked (để update)
        Task<Movies?> GetBySlugAsync(string slug, CancellationToken ct);
        Task<bool> ExistsAsync(int movieID, CancellationToken ct);

        Task<IReadOnlyList<Movies>> GetPagedAsync(int page, int pageSize, CancellationToken ct,
            string? keyword = null, string? status = null, string? type = null, int? year = null);

        Task<int> CountAsync(CancellationToken ct,
            string? keyword = null, string? status = null, string? type = null, int? year = null);

        // UPDATE
        Task UpdateAsync(Movies movie);                                 // Update entity đã tracked
        Task PatchAsync(int movieID, Action<Movies> apply, CancellationToken ct); // tải-tracked rồi áp thay đổi

        // DELETE
        Task RemoveAsync(int movieID);                                 // mark Deleted -> UoW sẽ commit
        Task<int> HardDeleteAsync(int movieID, CancellationToken ct);  // bulk delete ngay trên DB (EF Core 7+)
        Task<List<Movies>> GetAllMovieAsync(CancellationToken ct);
    }
    public sealed class MovieRepository : IMovieRepository
    {
        private readonly MovieDbContext _context;

        public MovieRepository(MovieDbContext context)
        {
            _context = context;
        }

        // ------------- CREATE -------------
        public Task AddAsync(Movies movie, CancellationToken ct)
            => _context.Movies.AddAsync(movie, ct).AsTask();

        // ------------- READ -------------
        // Đọc nhẹ, không tracking
        public Task<Movies?> GetByIdAsync(int movieID, CancellationToken ct)
            => _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieID, ct);

        // Dùng cho cập nhật: entity được track
        public Task<Movies?> GetTrackedAsync(int movieID, CancellationToken ct)
            => _context.Movies
                .FirstOrDefaultAsync(x => x.movieID == movieID, ct);

        public Task<Movies?> GetBySlugAsync(string slug, CancellationToken ct)
            => _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.slug == slug, ct);

        public Task<bool> ExistsAsync(int movieID, CancellationToken ct)
            => _context.Movies.AnyAsync(x => x.movieID == movieID, ct);

        // List + filter cơ bản + paging
        public async Task<IReadOnlyList<Movies>> GetPagedAsync(
            int page, int pageSize, CancellationToken ct,
            string? keyword = null, string? status = null, string? type = null, int? year = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            IQueryable<Movies> q = _context.Movies.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(m => EF.Functions.Like(m.title, $"%{keyword}%"));

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(m => m.status == status);

            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(m => m.movieType == type);

            if (year is not null)
                q = q.Where(m => m.year == year);

            // Sắp xếp: mới nhất trước (tuỳ bạn)
            q = q.OrderByDescending(m => m.releaseDate ?? m.updatedAt);

            return await q.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync(ct);
        }

        public Task<int> CountAsync(
            CancellationToken ct,
            string? keyword = null, string? status = null, string? type = null, int? year = null)
        {
            IQueryable<Movies> q = _context.Movies.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(m => EF.Functions.Like(m.title, $"%{keyword}%"));

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(m => m.status == status);

            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(m => m.movieType == type);

            if (year is not null)
                q = q.Where(m => m.year == year);

            return q.CountAsync(ct);
        }

        // ------------- UPDATE -------------
        // Cách 1: entity đã được track (lấy từ GetTrackedAsync), chỉ cần Update() nếu bạn attach entity mới
        public Task UpdateAsync(Movies movie)
        {
            // Nếu entity đã được DbContext track thì không cần gọi Update()
            // Gọi Update() sẽ đánh dấu Modified cho tất cả property -> cân nhắc mapping partial khi cần.
            _context.Movies.Update(movie);
            return Task.CompletedTask; // UoW sẽ SaveChangesAsync(ct)
        }

        // Cách 2: tải entity tracked rồi apply patch (ít chạm trường không đổi)
        public async Task PatchAsync(int movieID, Action<Movies> apply, CancellationToken ct)
        {
            var entity = await _context.Movies.FirstOrDefaultAsync(x => x.movieID == movieID, ct);
            if (entity is null) throw new KeyNotFoundException($"Movie {movieID} not found.");

            apply(entity); // áp thay đổi do caller truyền vào
            // Không SaveChanges ở đây; UoW sẽ commit
        }

        // ------------- DELETE -------------
        // Đánh dấu xoá (để UoW commit)
        public Task RemoveAsync(int movieID)
        {
            var stub = new Movies { movieID = movieID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }

        // Xoá nhanh trên DB (bulk) – tham gia transaction của UoW nếu có
        public Task<int> HardDeleteAsync(int movieID, CancellationToken ct)
            => _context.Movies
                       .Where(m => m.movieID == movieID)
                       .ExecuteDeleteAsync(ct);
        public Task<List<Movies>> GetAllMovieAsync(CancellationToken ct)
            => _context.Movies.AsNoTracking().ToListAsync(ct);
    }
}
