using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Infrastructure.Repository.People
{
    public interface IPersonRepository
    {
        Task AddAsync(Domain.People.Person person, CancellationToken ct);
        Task<Domain.People.Person?> GetByIdAsync(int personID, CancellationToken ct);
        Task<Domain.People.Person?> GetTrackedAsync(int personID, CancellationToken ct);
        Task<bool> ExistsAsync(int personID, CancellationToken ct);
        Task UpdateAsync(Domain.People.Person person, CancellationToken ct);
        Task<bool> PatchAsync(int personID, Action<Domain.People.Person> apply, CancellationToken ct);
        Task RemoveAsync(int personID);
        Task<int> HardDeleteAsync(int personID, CancellationToken ct);
        Task<List<Domain.People.Person>> GetAllPersonAsync(CancellationToken ct);
    }
    public sealed class PersonRepository : IPersonRepository
    {
        private readonly MovieDbContext _context;
        public PersonRepository(MovieDbContext context) => _context = context;
        public Task AddAsync(Domain.People.Person person, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(person);
            return _context.Persons.AddAsync(person, ct).AsTask();
        }
        public Task<Domain.People.Person?> GetByIdAsync(int personID, CancellationToken ct)
            => _context.Persons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.personID == personID, ct);
        public Task<Domain.People.Person?> GetTrackedAsync(int personID, CancellationToken ct)
            => _context.Persons.FirstOrDefaultAsync(x => x.personID == personID, ct);
        public Task<bool> ExistsAsync(int personID, CancellationToken ct)
            => _context.Persons.AsNoTracking().AnyAsync(x => x.personID == personID, ct);
        public Task UpdateAsync(Domain.People.Person person, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(person);
            _context.Persons.Update(person);
            return Task.CompletedTask;
        }
        public async Task<bool> PatchAsync(int personID, Action<Domain.People.Person> apply, CancellationToken ct)
        {
            var person = await GetTrackedAsync(personID, ct);
            if (person is null) return false;
            apply(person);
            return true;
        }
        public Task RemoveAsync(int personID)
        {
            var stub = new Domain.People.Person { personID = personID };
            _context.Entry(stub).State = EntityState.Deleted;
            return Task.CompletedTask;
        }
        public Task<int> HardDeleteAsync(int personID, CancellationToken ct)
            => _context.Persons
                .Where(x => x.personID == personID)
                .ExecuteDeleteAsync(ct);
        public Task<List<Domain.People.Person>> GetAllPersonAsync(CancellationToken ct)
            => _context.Persons.AsNoTracking().ToListAsync(ct);
    }
}
