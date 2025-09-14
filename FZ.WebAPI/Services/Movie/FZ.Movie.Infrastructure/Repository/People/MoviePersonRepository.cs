using FZ.Constant;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.People;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.People
{
    public interface IMoviePersonRepository
    {
        Task AddAsync(Domain.People.MoviePerson moviePerson, CancellationToken ct);
        Task<Domain.People.MoviePerson?> GetByIdAsync(int moviePersonID, CancellationToken ct);
        Task<Domain.People.MoviePerson?> GetTrackedAsync(int moviePersonID, CancellationToken ct);
        Task<bool> ExistsAsync(int moviePersonID, CancellationToken ct);
        Task UpdateAsync(Domain.People.MoviePerson moviePerson, CancellationToken ct);
        Task<bool> PatchAsync(int moviePersonID, Action<Domain.People.MoviePerson> apply, CancellationToken ct);
        Task RemoveAsync(int moviePersonID);
        Task<int> HardDeleteAsync(int moviePersonID, CancellationToken ct);
        Task<List<Domain.People.MoviePerson>> GetAllByPersonIdAsync(int personId, CancellationToken ct);
        Task<List<Domain.People.MoviePerson>> GetAllMoviePersonAsync(CancellationToken ct);
        Task<MoviePerson?> GetByMovieAndPersonIdAsync(int movieId, int personId, CancellationToken ct);
        Task <List<Person>> GetCreditsByMovieIdAsync(int movieId, CancellationToken ct);
        Task<List<Movies>> GetMoviesByPersonID(int personID, CancellationToken ct);
        Task<MoviePerson> GetByMovieIDAndPersonID(int movieID, int personID, CancellationToken ct);

        Task<List<MoviePerson>> GetMoviePersonByMovieID(int movieID, CancellationToken ct);



    }
    public sealed class MoviePersonRepository : IMoviePersonRepository
    {
        private readonly MovieDbContext _context;
        public MoviePersonRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.People.MoviePerson moviePerson, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(moviePerson);
            return _context.MoviePersons.AddAsync(moviePerson, ct).AsTask();
        }
        public Task<Domain.People.MoviePerson?> GetByIdAsync(int moviePersonID, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.moviePersonID == moviePersonID, ct);
        public Task<Domain.People.MoviePerson?> GetTrackedAsync(int moviePersonID, CancellationToken ct)
            => _context.MoviePersons.FirstOrDefaultAsync(x => x.moviePersonID == moviePersonID, ct);
        public Task<bool> ExistsAsync(int moviePersonID, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking().AnyAsync(x => x.moviePersonID == moviePersonID, ct);
        public Task UpdateAsync(Domain.People.MoviePerson moviePerson, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(moviePerson);
            _context.MoviePersons.Update(moviePerson);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int moviePersonID, Action<Domain.People.MoviePerson> apply, CancellationToken ct)
        {
            var moviePerson = await GetTrackedAsync(moviePersonID, ct);
            if (moviePerson is null) return false;
            apply(moviePerson);
            return true;
        }
        public Task RemoveAsync(int moviePersonID)
        {
            var stub = new Domain.People.MoviePerson { moviePersonID = moviePersonID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int moviePersonID, CancellationToken ct)
            => _context.MoviePersons
                .Where(x => x.moviePersonID == moviePersonID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.People.MoviePerson>> GetAllByPersonIdAsync(int personId, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .Where(x => x.personID == personId)
                .ToListAsync(ct);
        public Task<List<Domain.People.MoviePerson>> GetAllMoviePersonAsync(CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .ToListAsync(ct);
        public Task<MoviePerson?> GetByMovieAndPersonIdAsync(int movieId, int personId, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieId && x.personID == personId, ct);

        public Task<List<Person>> GetCreditsByMovieIdAsync(int movieId, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .Where(x => x.movieID == movieId)
                .Include(x => x.person)
                .Select(x => x.person)  
                .ToListAsync(ct);
        public Task<List<Movies>> GetMoviesByPersonID(int personID, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .Where(x => x.personID == personID)
                .Include(x => x.movie)
                .Select(x => x.movie)
                .ToListAsync(ct);
        public Task<MoviePerson> GetByMovieIDAndPersonID(int movieID, int personID, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieID && x.personID == personID, ct);

        public Task<List<MoviePerson>> GetMoviePersonByMovieID(int movieID, CancellationToken ct)
            => _context.MoviePersons.AsNoTracking()
                .Where(x => x.movieID == movieID)
                .ToListAsync(ct);


    }
}
