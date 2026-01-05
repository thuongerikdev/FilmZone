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
    public interface IRegionRepository
    {
        //Task<bool> IsRegionExistsAsync(string name, CancellationToken ct);
        //Task<bool> IsRegionExistsAsync(int regionID, CancellationToken ct);
        Task AddAsync(Region region, CancellationToken ct);
        Task<Region?> GetByIdAsync(int regionID, CancellationToken ct);
        Task UpdateAsync(Region region, CancellationToken ct);
        Task RemoveAsync(int regionID);

        Task<List<Region>> GetALLRegionMoviesAsync(CancellationToken ct);

        Task<List<Movies>> GetMoviesByRegionIDAsync(int regionID, CancellationToken ct);
        Task<List<Person>> GetPeopleByRegionID (int regionID, CancellationToken ct);

        Task <Region> GetByNameAsync(string name, CancellationToken ct);


    }
    public class RegionRepository : IRegionRepository
    {
        private readonly MovieDbContext _dbContext;
        public RegionRepository(MovieDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(Region person, CancellationToken ct)
        {
            await _dbContext.Set<Region>().AddAsync(person, ct);
        }

        public async Task<Domain.People.Region?> GetByIdAsync(int regionID, CancellationToken ct)
        {
            return await _dbContext.Set<Domain.People.Region>()
                .FindAsync(new object[] { regionID }, ct);
        }
        public async Task UpdateAsync(Domain.People.Region region, CancellationToken ct)
        {
            _dbContext.Set<Domain.People.Region>().Update(region);
            await Task.CompletedTask;
        }
        public async Task RemoveAsync(int regionID)
        {
            var region = await _dbContext.Set<Domain.People.Region>().FindAsync(regionID);
            if (region != null)
            {
                _dbContext.Set<Domain.People.Region>().Remove(region);
            }
        }
        public async Task<List<Region>> GetALLRegionMoviesAsync(CancellationToken ct)
        {
            return await _dbContext.Set<Region>()
                .ToListAsync(ct);
        }
        public async Task<List<Movies>> GetMoviesByRegionIDAsync(int regionID, CancellationToken ct)
        {
            return await _dbContext.Set<Movies>()
                .Where(m => m.regionID == regionID)
                .ToListAsync(ct);
        }
      

        public async Task<List<Person>> GetPeopleByRegionID(int regionID, CancellationToken ct)
        {
            return await _dbContext.Set<Person>()
                .Where(m => m.regionID == regionID)
                .ToListAsync(ct);
        }
        public async Task<Region> GetByNameAsync(string name, CancellationToken ct)
        {
            return await _dbContext.Set<Region>()
                .FirstOrDefaultAsync(r => r.name == name, ct);
        }


    }
       
        
}
