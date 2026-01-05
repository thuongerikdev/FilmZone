using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Interactions
{
    public interface IUserRatingRepository
    {
        Task AddAsync(Domain.Interactions.UserRating userRating, CancellationToken ct);
        Task<Domain.Interactions.UserRating?> GetByIdAsync(int userRatingID, CancellationToken ct);
        Task<Domain.Interactions.UserRating?> GetTrackedAsync(int userRatingID, CancellationToken ct);
        Task<bool> ExistsAsync(int userRatingID, CancellationToken ct);
        Task UpdateAsync(Domain.Interactions.UserRating userRating, CancellationToken ct);
        Task<bool> PatchAsync(int userRatingID, Action<Domain.Interactions.UserRating> apply, CancellationToken ct);
        Task RemoveAsync(int userRatingID);
        Task<int> HardDeleteAsync(int userRatingID, CancellationToken ct);
        Task<List<Domain.Interactions.UserRating>> GetAllByUserIdAsync(int userId, CancellationToken ct);
        Task<List<Domain.Interactions.UserRating>> GetAllUserRatingAsync(CancellationToken ct);
        Task<List<Domain.Interactions.UserRating>> GetAllByMovieIDAsync(int movieId, CancellationToken ct);

    }
    public sealed class UserRatingRepository : IUserRatingRepository
    {
        private readonly MovieDbContext _context;
        public UserRatingRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.Interactions.UserRating userRating, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(userRating);
            return _context.UserRatings.AddAsync(userRating, ct).AsTask();
        }
        public Task<Domain.Interactions.UserRating?> GetByIdAsync(int userRatingID, CancellationToken ct)
            => _context.UserRatings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.userRatingID == userRatingID, ct);
        public Task<Domain.Interactions.UserRating?> GetTrackedAsync(int userRatingID, CancellationToken ct)
            => _context.UserRatings.FirstOrDefaultAsync(x => x.userRatingID == userRatingID, ct);
        public Task<bool> ExistsAsync(int userRatingID, CancellationToken ct)
            => _context.UserRatings.AsNoTracking().AnyAsync(x => x.userRatingID == userRatingID, ct);
        public Task UpdateAsync(Domain.Interactions.UserRating userRating, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(userRating);
            _context.UserRatings.Update(userRating);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int userRatingID, Action<Domain.Interactions.UserRating> apply, CancellationToken ct)
        {
            var userRating = await GetTrackedAsync(userRatingID, ct);
            if (userRating is null) return false;
            apply(userRating);
            return true;
        }
        public  Task RemoveAsync(int userRatingID)
        {
          var stub = new Domain.Interactions.UserRating { userRatingID = userRatingID };
            _context.Entry(stub).State = EntityState.Deleted;
            return  Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int userRatingID, CancellationToken ct)
            => _context.UserRatings.Where(c => c.userRatingID == userRatingID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.Interactions.UserRating>> GetAllByUserIdAsync(int userId, CancellationToken ct)
            => _context.UserRatings.AsNoTracking()
                .Where(ur => ur.userID == userId)
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.UserRating>> GetAllUserRatingAsync(CancellationToken ct)
            => _context.UserRatings.AsNoTracking()
                .ToListAsync(ct);
        public Task<List<Domain.Interactions.UserRating>> GetAllByMovieIDAsync(int movieId, CancellationToken ct)
            => _context.UserRatings.AsNoTracking()
                .Where(ur => ur.movieID == movieId)
                .ToListAsync(ct);
    }
}
