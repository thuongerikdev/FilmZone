using FZ.Movie.Domain.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Interactions
{
    public interface  ISavedMovieRepository
    {
        Task AddAsync(Domain.Interactions.SavedMovie savedMovie, CancellationToken ct);
        Task<Domain.Interactions.SavedMovie?> GetByIdAsync(int savedMovieID, CancellationToken ct);
        Task<Domain.Interactions.SavedMovie?> GetTrackedAsync(int savedMovieID, CancellationToken ct);
        Task<bool> ExistsAsync(int savedMovieID, CancellationToken ct);
        Task UpdateAsync(Domain.Interactions.SavedMovie savedMovie, CancellationToken ct);
        Task<bool> PatchAsync(int savedMovieID, Action<Domain.Interactions.SavedMovie> apply, CancellationToken ct);
        Task RemoveAsync(int savedMovieID);
        Task<int> HardDeleteAsync(int savedMovieID, CancellationToken ct);
        Task<List<Domain.Interactions.SavedMovie>> GetAllByUserIdAsync(int userId, CancellationToken ct);
        Task<List<Domain.Interactions.SavedMovie>> GetAllSavedMovieAsync(CancellationToken ct);
        Task<List<Domain.Interactions.SavedMovie>> GetAllByMovieIDAsync(int movieId, CancellationToken ct);
        Task <SavedMovie?> GetByUserAndMovieIDAsync(int userId, int movieId, CancellationToken ct);
    }
    public sealed class SavedMovieRepository : ISavedMovieRepository
    {
        private readonly MovieDbContext _context;
        public SavedMovieRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.Interactions.SavedMovie savedMovie, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(savedMovie);
            return _context.SavedMovies.AddAsync(savedMovie, ct).AsTask();
        }
        public Task<Domain.Interactions.SavedMovie?> GetByIdAsync(int savedMovieID, CancellationToken ct)
            => _context.SavedMovies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.savedMovieID == savedMovieID, ct);
        public Task<Domain.Interactions.SavedMovie?> GetTrackedAsync(int savedMovieID, CancellationToken ct)
            => _context.SavedMovies.FirstOrDefaultAsync(x => x.savedMovieID == savedMovieID, ct);
        public Task<bool> ExistsAsync(int savedMovieID, CancellationToken ct)
            => _context.SavedMovies.AsNoTracking().AnyAsync(x => x.savedMovieID == savedMovieID, ct);
        public Task UpdateAsync(Domain.Interactions.SavedMovie savedMovie, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(savedMovie);
            _context.SavedMovies.Update(savedMovie);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int savedMovieID, Action<Domain.Interactions.SavedMovie> apply, CancellationToken ct)
        {
            var savedMovie = await GetTrackedAsync(savedMovieID, ct);
            if (savedMovie is null) return false;
            apply(savedMovie);
            return true;
        }
        public  Task RemoveAsync(int savedMovieID)
        {
            var stub = new Domain.Interactions.SavedMovie { savedMovieID = savedMovieID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;

        }
        public Task<int> HardDeleteAsync(int savedMovieID, CancellationToken ct)
            => _context.SavedMovies.Where(c => c.savedMovieID == savedMovieID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Interactions.SavedMovie>> GetAllByUserIdAsync(int userId, CancellationToken ct)
            => _context.SavedMovies.AsNoTracking()
                .Where(sm => sm.userID == userId)
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.SavedMovie>> GetAllSavedMovieAsync(CancellationToken ct)
            => _context.SavedMovies.AsNoTracking()
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.SavedMovie>> GetAllByMovieIDAsync(int movieId, CancellationToken ct)
            => _context.SavedMovies.AsNoTracking()
                .Where(sm => sm.movieID == movieId)
                .ToListAsync(ct);
        public Task<SavedMovie?> GetByUserAndMovieIDAsync(int userId, int movieId, CancellationToken ct)
            => _context.SavedMovies.AsNoTracking()
                .FirstOrDefaultAsync(sm => sm.userID == userId && sm.movieID == movieId, ct);
    }
}
