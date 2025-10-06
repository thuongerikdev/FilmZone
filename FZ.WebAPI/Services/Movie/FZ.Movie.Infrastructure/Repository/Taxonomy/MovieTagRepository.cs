using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Taxonomy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Taxonomy
{
    public interface IMovieTagRepository
    {
        Task AddAsync(Domain.Taxonomy.MovieTag movieTag, CancellationToken ct);
        Task<Domain.Taxonomy.MovieTag?> GetByIdAsync(int movieTagID, CancellationToken ct);
        Task<Domain.Taxonomy.MovieTag?> GetTrackedAsync(int movieTagID, CancellationToken ct);
        Task<bool> ExistsAsync(int movieTagID, CancellationToken ct);
        Task UpdateAsync(Domain.Taxonomy.MovieTag movieTag, CancellationToken ct);
        Task<bool> PatchAsync(int movieTagID, Action<Domain.Taxonomy.MovieTag> apply, CancellationToken ct);
        Task RemoveAsync(int movieTagID);
        Task<int> HardDeleteAsync(int movieTagID, CancellationToken ct);
        Task<List<Domain.Taxonomy.MovieTag>> GetAllMovieTagAsync(CancellationToken ct);
        Task<MovieTag> GetByMovieAndTagAsync(int movieID, int tagID, CancellationToken ct);
        Task <MovieTag> GetByMovieID(int movieID,  CancellationToken ct);

        Task<List<MovieTag>> GetByMovieIDsync(int movieID, CancellationToken ct);

        Task<List<Tag>> GetTagByMovieID (int movieID, CancellationToken ct);
        Task<List<Movies>> GetMovieByTagID(List<int> tagID, CancellationToken ct);
        Task<List<MovieTag>> GetByTagID(int tagID, CancellationToken ct);
    }
    public sealed class MovieTagRepository : IMovieTagRepository
    {
        private readonly MovieDbContext _context;
        public MovieTagRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.Taxonomy.MovieTag movieTag, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(movieTag);
            return _context.MovieTags.AddAsync(movieTag, ct).AsTask();
        }
        public Task<Domain.Taxonomy.MovieTag?> GetByIdAsync(int movieTagID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieTagID == movieTagID, ct);
        public Task<Domain.Taxonomy.MovieTag?> GetTrackedAsync(int movieTagID, CancellationToken ct)
            => _context.MovieTags.FirstOrDefaultAsync(x => x.movieTagID == movieTagID, ct);
        public Task<bool> ExistsAsync(int movieTagID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking().AnyAsync(x => x.movieTagID == movieTagID, ct);
        public Task UpdateAsync(Domain.Taxonomy.MovieTag movieTag, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(movieTag);
            _context.MovieTags.Update(movieTag);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int movieTagID, Action<Domain.Taxonomy.MovieTag> apply, CancellationToken ct)
        {
            var movieTag = await GetTrackedAsync(movieTagID, ct);
            if (movieTag is null) return false;
            apply(movieTag);
            return true;
        }
        public Task RemoveAsync(int movieTagID)
        {
            var stub = new Domain.Taxonomy.MovieTag { movieTagID = movieTagID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int movieTagID, CancellationToken ct)
            => _context.MovieTags
                .Where(x => x.movieTagID == movieTagID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Taxonomy.MovieTag>> GetAllMovieTagAsync(CancellationToken ct)
            => _context.MovieTags.AsNoTracking().ToListAsync(ct);

        public Task<MovieTag> GetByMovieAndTagAsync(int movieID, int tagID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieID && x.tagID == tagID, ct);

        public Task<List<Tag>> GetTagByMovieID(int movieID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .Where(x => x.movieID == movieID)
                .Include(x => x.tag)
                .Select(x => x.tag)
                .ToListAsync(ct);
        public Task<List<Movies>> GetMovieByTagID(List<int> tagID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .Where(x => tagID.Contains(x.tagID))
                .Include(x => x.movie)
                .Select(x => x.movie)
                .ToListAsync(ct);
        public Task<List<MovieTag>> GetByMovieIDsync(int movieID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .Where(x => x.movieID == movieID)
                .ToListAsync(ct);


        public Task<MovieTag> GetByMovieID(int movieID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieID, ct);
        public Task<List<MovieTag>> GetByTagID(int tagID, CancellationToken ct)
            => _context.MovieTags.AsNoTracking()
                .Where(x => x.tagID == tagID)
                .ToListAsync(ct);




    }
}
