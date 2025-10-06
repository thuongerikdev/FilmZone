using FZ.Movie.Dtos.ElasticSearchDoc;
using FZ.Movie.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenSearch.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Search
{
    public interface IPersonIndexService
    {
        Task IndexByIdAsync(int personId, CancellationToken ct = default);
        Task DeleteAsync(int personId, CancellationToken ct = default);
    }

    public sealed class PersonIndexService : IPersonIndexService
    {
        private readonly MovieDbContext _db;
        private readonly IOpenSearchClient _os;
        private readonly string _indexName;

        public PersonIndexService(MovieDbContext db, IOpenSearchClient os, IConfiguration cfg)
        {
            _db = db;
            _os = os;
            _indexName = cfg["OpenSearch:PersonsIndex"]!;
        }

        public async Task IndexByIdAsync(int personId, CancellationToken ct = default)
        {
            var prs = await _db.Persons
                .Include(x => x.region)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.personID == personId, ct);

            if (prs is null)
            {
                await DeleteAsync(personId, ct);
                return;
            }

            var credits = await _db.MoviePersons
                .Include(mp => mp.movie)
                .Where(mp => mp.personID == personId)
                .AsNoTracking()
                .Select(mp => new PersonDoc.CreditMini
                {
                    MovieId = mp.movieID,
                    Title = mp.movie.title,
                    Year = mp.movie.year,
                    Role = mp.role,
                    CharacterName = mp.characterName,
                    CreditOrder = mp.creditOrder
                })
                .ToListAsync(ct);

            var doc = new PersonDoc
            {
                Id = prs.personID.ToString(),
                FullName = prs.fullName,
                KnownFor = prs.knownFor,
                RegionId = prs.regionID,
                RegionCode = prs.region?.code,
                RegionName = prs.region?.name,
                Biography = prs.biography,
                Avatar = prs.avatar,
                BirthDate = prs.birthDate,
                Credits = credits,
                UpdatedAt = prs.updatedAt
            };

            var resp = await _os.IndexAsync<PersonDoc>(doc, i => i
                .Index(_indexName)
                .Id(doc.Id), ct);

            if (!resp.IsValid)
                throw new Exception($"OS index person failed: {resp.DebugInformation}");
        }

        public Task DeleteAsync(int personId, CancellationToken ct = default)
        {
            var req = new DeleteRequest(_indexName, personId.ToString());
            return _os.DeleteAsync(req, ct);
        }
    }
}