using FZ.Movie.Domain.Media;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IMovieSubTitleRepository
    {
        // CREATE
        Task AddAsync(Domain.Media.MovieSubTitle movieSubTitle, CancellationToken ct);
        // READ
        Task<Domain.Media.MovieSubTitle?> GetByIdAsync(int movieSubTitleID, CancellationToken ct);                  // no-tracking (để đọc)
        Task<Domain.Media.MovieSubTitle?> GetTrackedAsync(int movieSubTitleID, CancellationToken ct);               // tracked (để update)
        Task<bool> ExistsAsync(int movieSubTitleID, CancellationToken ct);
      
        // UPDATE
        Task UpdateAsync(Domain.Media.MovieSubTitle movieSubTitle);                                 // Update entity đã tracked
        Task PatchAsync(int movieSubTitleID, Action<Domain.Media.MovieSubTitle> apply, CancellationToken ct); // tải-tracked rồi áp thay đổi
        // DELETE
        Task RemoveAsync(int movieSubTitleID);                                 // mark Deleted -> UoW sẽ commit
        Task<int> HardDeleteAsync(int movieSubTitleID, CancellationToken ct);  // bulk delete ngay trên DB (EF Core 7+)

        Task<List<Domain.Media.MovieSubTitle>> GetAllMovieSubTitleAsync(CancellationToken ct);
        Task<List<MovieSubTitle>> GetByMovieSourceIDAsync(int movieSourceID, CancellationToken ct);
    }

    public class MovieSubTitleRepository : IMovieSubTitleRepository { 
        private readonly MovieDbContext _context;
        public MovieSubTitleRepository(MovieDbContext context)
        {
            _context = context;
        }
        // ------------- CREATE -------------
        public Task AddAsync(Domain.Media.MovieSubTitle movieSubTitle, CancellationToken ct)
            => _context.MovieSubTitles.AddAsync(movieSubTitle, ct).AsTask();

        // ------------- READ -------------
        // Đọc nhẹ, không tracking
        public Task<Domain.Media.MovieSubTitle?> GetByIdAsync(int movieSubTitleID, CancellationToken ct)
            => _context.MovieSubTitles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieSubTitleID == movieSubTitleID, ct);
        // Dùng cho cập nhật: entity được track
        public Task<Domain.Media.MovieSubTitle?> GetTrackedAsync(int movieSubTitleID, CancellationToken ct)
            => _context.MovieSubTitles
                .FirstOrDefaultAsync(x => x.movieSubTitleID == movieSubTitleID, ct);
        public Task<bool> ExistsAsync(int movieSubTitleID, CancellationToken ct)
            => _context.MovieSubTitles.AsNoTracking().AnyAsync(x => x.movieSubTitleID == movieSubTitleID, ct);


        // ------------- UPDATE -------------
        public Task UpdateAsync(Domain.Media.MovieSubTitle movieSubTitle)
        {
            _context.MovieSubTitles.Update(movieSubTitle);
            return Task.CompletedTask;
        }
        public async Task PatchAsync(int movieSubTitleID, Action<Domain.Media.MovieSubTitle> apply, CancellationToken ct)
        {
            var entity =  await GetTrackedAsync(movieSubTitleID, ct);
            if (entity != null)
            {
                apply(entity);
            }
        }
        // ------------- DELETE -------------
        public Task RemoveAsync(int movieSubTitleID)
        {
            var entity = new Domain.Media.MovieSubTitle { movieSubTitleID = movieSubTitleID };
            _context.MovieSubTitles.Attach(entity);
            _context.MovieSubTitles.Remove(entity);
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int movieSubTitleID, CancellationToken ct)
             => _context.MovieSources.Where(c => c.movieSourceID == movieSubTitleID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Media.MovieSubTitle>> GetAllMovieSubTitleAsync(CancellationToken ct)
            => _context.MovieSubTitles.AsNoTracking().ToListAsync(ct);
        public Task<List<MovieSubTitle>> GetByMovieSourceIDAsync(int movieSourceID, CancellationToken ct)
            => _context.MovieSubTitles.AsNoTracking()
                .Where(x => x.movieSourceID == movieSourceID)
                .ToListAsync(ct);



    }

}
