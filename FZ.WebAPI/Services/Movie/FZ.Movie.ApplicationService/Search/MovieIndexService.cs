using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Client;                           // ⬅️ dùng OpenSearch
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Dtos.ElasticSearchDoc;
using FZ.Movie.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FZ.Movie.ApplicationService.Search
{
    public interface IMovieIndexService
    {
        Task IndexByIdAsync(int movieId, CancellationToken ct = default);
        Task DeleteAsync(int movieId, CancellationToken ct = default);
        Task BulkIndexByIdsAsync(IEnumerable<int> movieIds, CancellationToken ct = default);
        Task ReindexByPersonAsync(int personId, CancellationToken ct = default);
        Task ReindexByTagAsync(int tagId, CancellationToken ct = default);
        Task ReindexByRegionAsync(int regionId, CancellationToken ct = default);
    }

    public sealed class MovieIndexService : IMovieIndexService
    {
        private readonly MovieDbContext _db;
        private readonly IOpenSearchClient _os;
        private readonly string _indexName;

        public MovieIndexService(MovieDbContext db, IOpenSearchClient os, IConfiguration cfg)
        {
            _db = db;
            _os = os;
            _indexName = cfg["OpenSearch:MoviesIndex"]!;
        }

        public async Task IndexByIdAsync(int movieId, CancellationToken ct = default)
        {
            var mv = await _db.Movies
                .Include(x => x.movieTags).ThenInclude(mt => mt.tag)
                .Include(x => x.credits).ThenInclude(mp => mp.person)
                .Include(x => x.regions)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.movieID == movieId, ct);

            if (mv is null)
            {
                await DeleteAsync(movieId, ct);
                return;
            }

            var doc = MapToDoc(mv);

            var resp = await _os.IndexAsync<MovieDoc>(doc, i => i
                .Index(_indexName)
                .Id(doc.Id), ct);

            if (!resp.IsValid)
                throw new Exception($"OS index movie failed: {resp.DebugInformation}");
        }

        public Task DeleteAsync(int movieId, CancellationToken ct = default)
        {
            var req = new DeleteRequest(_indexName, movieId.ToString());
            return _os.DeleteAsync(req, ct);
        }

        public async Task BulkIndexByIdsAsync(IEnumerable<int> movieIds, CancellationToken ct = default)
        {
            var ids = movieIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) return; // ✅ Không có movie thì bỏ qua

            var movies = await _db.Movies
                .Where(x => ids.Contains(x.movieID))
                .Include(x => x.movieTags).ThenInclude(mt => mt.tag)
                .Include(x => x.credits).ThenInclude(mp => mp.person)
                .Include(x => x.regions)
                .AsNoTracking()
                .ToListAsync(ct);

            if (movies.Count == 0) return; // ✅ Không có movie hợp lệ thì bỏ qua

            var docs = movies.Select(MapToDoc).ToList();
            if (docs.Count == 0) return; // ✅ Không có doc để index thì bỏ qua

            var resp = await _os.BulkAsync(b =>
            {
                foreach (var d in docs)
                    b.Index<MovieDoc>(bi => bi.Index(_indexName).Id(d.Id).Document(d));
                return b;
            }, ct);

            if (!resp.IsValid || resp.Errors)
                throw new Exception($"OS bulk index movies failed: {resp.DebugInformation}");
        }

        public async Task ReindexByPersonAsync(int personId, CancellationToken ct = default)
        {
            var ids = await _db.MoviePersons
                .Where(x => x.personID == personId)
                .Select(x => x.movieID)
                .Distinct()
                .ToListAsync(ct);

            if (ids.Count == 0) return; // ✅ Không có movie liên quan thì bỏ qua
            await BulkIndexByIdsAsync(ids, ct);
        }

        public async Task ReindexByTagAsync(int tagId, CancellationToken ct = default)
        {
            var ids = await _db.MovieTags
                .Where(x => x.tagID == tagId)
                .Select(x => x.movieID)
                .Distinct()
                .ToListAsync(ct);

            if (ids.Count == 0) return; // ✅ Không có movie liên quan thì bỏ qua
            await BulkIndexByIdsAsync(ids, ct);
        }

        public async Task ReindexByRegionAsync(int regionId, CancellationToken ct = default)
        {
            var ids = await _db.Movies
                .Where(x => x.regionID == regionId)
                .Select(x => x.movieID)
                .Distinct()
                .ToListAsync(ct);

            if (ids.Count == 0) return; // ✅ Không có movie liên quan thì bỏ qua
            await BulkIndexByIdsAsync(ids, ct);
        }

        private static MovieDoc MapToDoc(Movies mv)
        {
            return new MovieDoc
            {
                Id = mv.movieID.ToString(),
                Slug = mv.slug,
                Title = mv.title,
                OriginalTitle = mv.originalTitle,
                Description = mv.description,
                MovieType = mv.movieType,
                Image = mv.image,
                Status = mv.status,
                ReleaseDate = mv.releaseDate,
                DurationSeconds = mv.durationSeconds,
                TotalSeasons = mv.totalSeasons,
                TotalEpisodes = mv.totalEpisodes,

                RegionId = mv.regionID,
                RegionCode = mv.regions?.code,
                RegionName = mv.regions?.name,

                Year = mv.year,
                Rated = mv.rated,
                Popularity = mv.popularity,

                Tags = mv.movieTags?.Select(t => new MovieDoc.TagMini
                {
                    TagId = t.tagID,
                    TagName = t.tag.tagName,
                    Slug = (t.tag.tagName ?? "").Trim().ToLower().Replace(' ', '-')
                }).ToList() ?? new(),

                Cast = mv.credits?.Select(c => new MovieDoc.CastMini
                {
                    PersonId = c.personID,
                    FullName = c.person.fullName,
                    Role = c.role,
                    CharacterName = c.characterName,
                    CreditOrder = c.creditOrder
                }).ToList() ?? new(),

                UpdatedAt = mv.updatedAt
            };
        }
    }
}