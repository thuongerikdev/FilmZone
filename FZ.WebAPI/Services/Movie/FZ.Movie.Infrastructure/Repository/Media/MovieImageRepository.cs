using FZ.Movie.Domain.Media;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.Media
{
    public interface IMovieImageRepository
    {
        Task AddMovieImageAsync(Domain.Media.MovieImage movieImage, CancellationToken ct);
    
        Task RemoveAsync(int movieImageID);
        Task<List<MovieImage>> GetByMovieID(int movieID, CancellationToken ct);
        
    }
    public sealed class MovieImageRepository : IMovieImageRepository
    {
        private readonly MovieDbContext _context;
        public MovieImageRepository(MovieDbContext context) => _context = context;
        public Task AddMovieImageAsync(Domain.Media.MovieImage movieImage, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(movieImage);
            return _context.MovieImages.AddAsync(movieImage, ct).AsTask();
        }
        public  Task RemoveAsync(int movieImageID)
        {
           var stub = new Domain.Media.MovieImage { movieImageID = movieImageID };
              _context.Entry(stub).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                return Task.CompletedTask;
        }
        public async Task<List<MovieImage>> GetByMovieID(int movieID, CancellationToken ct)
        {
            return await _context.MovieImages.Where(mi => mi.movieID == movieID).ToListAsync(ct);
        }
    }
}
