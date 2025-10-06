using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch; // vẫn inject client nếu bạn cần chỗ khác
using FZ.Movie.Dtos.ElasticSearchDoc;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Search
{
    [ApiController]
    [Route("api/search")]
    public sealed class SearchController : ControllerBase
    {
        private readonly ElasticsearchClient _es; // vẫn giữ nếu nơi khác cần
        private readonly string _moviesIdx;
        private readonly string _personsIdx;
        private readonly string _esBaseUrl;
        private readonly string? _esUser;
        private readonly string? _esPass;

        public SearchController(ElasticsearchClient es, IConfiguration cfg)
        {
            _es = es;
            var esCfg = cfg.GetSection("OpenSearch");
            _moviesIdx = esCfg["MoviesIndex"]!;
            _personsIdx = esCfg["PersonsIndex"]!;
            _esBaseUrl = esCfg["Url"]!.TrimEnd('/');
            _esUser = esCfg["Username"];
            _esPass = esCfg["Password"];
        }

        // Helper: HttpClient với Basic Auth (nếu có)
        private HttpClient CreateHttp()
        {
            var http = new HttpClient { BaseAddress = new Uri(_esBaseUrl + "/") };
            if (!string.IsNullOrWhiteSpace(_esUser) && _esPass is not null)
            {
                var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_esUser}:{_esPass}"));
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
            return http;
        }

        // Helper: POST JSON
        private static async Task<JsonDocument> PostJsonAsync(HttpClient http, string path, object body, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(body);
            using var req = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            using var res = await http.SendAsync(req, ct);
            var txt = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
                throw new Exception($"ES {path} failed ({(int)res.StatusCode}): {txt}");
            return JsonDocument.Parse(txt);
        }

        // ===================== SEARCH MOVIES =====================
        [HttpGet("movies")]
        public async Task<IActionResult> SearchMovies(
            [FromQuery] string? q,
            [FromQuery] int? personId,
            [FromQuery] int[]? tagIds,
            [FromQuery] string? regionCode,
            [FromQuery] int page = 1,
            [FromQuery] int size = 12,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            size = Math.Clamp(size, 1, 100);

            // Build bool.must []
            var must = new List<object>();

            if (!string.IsNullOrWhiteSpace(q))
            {
                must.Add(new
                {
                    multi_match = new
                    {
                        query = q,
                        fields = new[] { "title^3", "title.auto^5", "description" },
                        fuzziness = "AUTO"
                    }
                });
            }

            if (personId.HasValue)
            {
                must.Add(new
                {
                    nested = new
                    {
                        path = "cast",
                        query = new
                        {
                            term = new Dictionary<string, object>
                            {
                                ["cast.personId"] = personId.Value
                            }
                        }
                    }
                });
            }

            if (tagIds is { Length: > 0 })
            {
                must.Add(new
                {
                    nested = new
                    {
                        path = "tags",
                        query = new
                        {
                            terms = new Dictionary<string, object>
                            {
                                ["tags.tagId"] = tagIds
                            }
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(regionCode))
            {
                must.Add(new
                {
                    term = new Dictionary<string, object>
                    {
                        ["regionCode"] = regionCode
                    }
                });
            }

            var query = new { @bool = new { must } };

            var http = CreateHttp();

            // _search (paged)
            var searchBody = new
            {
                from = (page - 1) * size,
                size,
                query,
                sort = new object[]
                {
                    new Dictionary<string, object> { ["popularity"] = new { order = "desc" } },
                    new Dictionary<string, object> { ["updatedAt"]  = new { order = "desc" } }
                }
                // highlight có thể thêm sau
            };
            using var searchDoc = await PostJsonAsync(http, $"{_moviesIdx}/_search", searchBody, ct);

            // _count (tổng)
            var countBody = new { query };
            using var countDoc = await PostJsonAsync(http, $"{_moviesIdx}/_count", countBody, ct);
            var total = countDoc.RootElement.GetProperty("count").GetInt64();

            // Parse hits -> MovieDoc
            var items = new List<object>();
            var hits = searchDoc.RootElement.GetProperty("hits").GetProperty("hits");
            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var doc = JsonSerializer.Deserialize<MovieDoc>(src)!;
                double? score = h.TryGetProperty("_score", out var sc) && sc.ValueKind is JsonValueKind.Number
                    ? sc.GetDouble()
                    : null;

                items.Add(new
                {
                    id = doc.Id,
                    slug = doc.Slug,
                    title = doc.Title,
                    year = doc.Year,
                    region = new { id = doc.RegionId, code = doc.RegionCode, name = doc.RegionName },
                    tags = doc.Tags,
                    popularity = doc.Popularity,
                    score
                });
            }

            return Ok(new { total, page, size, items });
        }

        // ===================== SUGGEST MOVIES =====================
        [HttpGet("movies/suggest")]
        public async Task<IActionResult> SuggestMovies([FromQuery] string q, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());

            var http = CreateHttp();
            var body = new
            {
                size = 8,
                query = new
                {
                    match = new Dictionary<string, object>
                    {
                        ["title.auto"] = new { query = q }
                    }
                },
                _source = new[] { "id", "slug", "title", "year" }
            };

            using var doc = await PostJsonAsync(http, $"{_moviesIdx}/_search", body, ct);

            var items = new List<object>();
            var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var d = JsonSerializer.Deserialize<MovieDoc>(src)!;
                items.Add(new { d.Id, d.Slug, d.Title, d.Year });
            }

            return Ok(items);
        }

        // ===================== SEARCH PERSONS =====================
        [HttpGet("persons")]
        public async Task<IActionResult> SearchPersons([FromQuery] string q, [FromQuery] string? regionCode = null, CancellationToken ct = default)
        {
            var must = new List<object>
            {
                new
                {
                    multi_match = new
                    {
                        query = q,
                        fields = new[] { "fullName^3", "fullName.auto^5", "knownFor", "biography" }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(regionCode))
            {
                must.Add(new
                {
                    term = new Dictionary<string, object>
                    {
                        ["regionCode"] = regionCode
                    }
                });
            }

            var query = new { @bool = new { must } };

            var http = CreateHttp();

            var body = new
            {
                size = 20,
                query
            };

            using var doc = await PostJsonAsync(http, $"{_personsIdx}/_search", body, ct);

            var items = new List<object>();
            var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var d = JsonSerializer.Deserialize<PersonDoc>(src)!;
                items.Add(new
                {
                    id = d.Id,
                    fullName = d.FullName,
                    region = new { d.RegionId, d.RegionCode, d.RegionName },
                    knownFor = d.KnownFor
                });
            }

            return Ok(items);
        }
    }
}
