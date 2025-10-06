﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FZ.Movie.ApplicationService.Search;
using FZ.Movie.Dtos.ElasticSearchDoc;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Search
{
    [ApiController]
    [Route("api/search")]
    public sealed class SearchController : ControllerBase
    {
        // ✅ Case-insensitive khi deserialize _source -> MovieDoc/PersonDoc
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _moviesIdx;
        private readonly string _personsIdx;
        private readonly string _esBaseUrl;
        private readonly string? _esUser;
        private readonly string? _esPass;
        private readonly IMovieIndexService _movieIndexService;

        public SearchController(IConfiguration cfg, IMovieIndexService movieIndexService)
        {
            var esCfg = cfg.GetSection("OpenSearch");
            _moviesIdx = esCfg["MoviesIndex"]!;
            _personsIdx = esCfg["PersonsIndex"]!;
            _esBaseUrl = esCfg["Url"]!.TrimEnd('/');
            _esUser = esCfg["Username"];
            _esPass = esCfg["Password"];
            _movieIndexService = movieIndexService;
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

        // Helper: POST JSON -> JsonDocument
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

            var filterMust = new List<object>();   // chỉ chứa các filter không liên quan full-text
            var qShould = new List<object>();      // toàn bộ điều kiện liên quan đến q
            var qs = q?.Trim();
            var hasQ = !string.IsNullOrWhiteSpace(qs);
            var qLen = hasQ ? qs!.Length : 0;
            var qLower = hasQ ? qs!.ToLowerInvariant() : null;

            // ========== FILTERS ==========
            if (personId.HasValue && personId.Value > 0)
            {
                filterMust.Add(new
                {
                    nested = new
                    {
                        path = "cast",
                        query = new
                        {
                            term = new Dictionary<string, object> { ["cast.personId"] = personId.Value }
                        }
                    }
                });
            }

            if (tagIds is { Length: > 0 })
            {
                filterMust.Add(new
                {
                    nested = new
                    {
                        path = "tags",
                        query = new
                        {
                            terms = new Dictionary<string, object> { ["tags.tagId"] = tagIds }
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(regionCode))
            {
                filterMust.Add(new { term = new Dictionary<string, object> { ["regionCode"] = regionCode! } });
            }

            // ========== FULL-TEXT: q ==========
            if (hasQ)
            {
                // 1) Văn bản cấp document (không nested)
                if (qLen <= 2)
                {
                    // prefix cho chuỗi ngắn
                    qShould.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["title"] = new { query = qs, max_expansions = 50 } } });
                    qShould.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["originalTitle"] = new { query = qs, max_expansions = 50 } } });
                    qShould.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["description"] = new { query = qs, max_expansions = 50 } } });
                    qShould.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["regionName"] = new { query = qs, max_expansions = 50 } } });
                    qShould.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["slug"] = new { query = qs, max_expansions = 50 } } });
                }
                else
                {
                    // multi_match + fuzziness cho chuỗi dài
                    qShould.Add(new
                    {
                        multi_match = new
                        {
                            query = qs,
                            fields = new[] { "title^6", "originalTitle^4", "description", "regionName", "slug^2" },
                            fuzziness = "AUTO"
                        }
                    });
                }

                // 2) Nested: cast.fullName
                if (qLen <= 2)
                {
                    qShould.Add(new
                    {
                        nested = new
                        {
                            path = "cast",
                            query = new
                            {
                                match_phrase_prefix = new Dictionary<string, object>
                                {
                                    ["cast.fullName"] = new { query = qs, max_expansions = 50 }
                                }
                            }
                        }
                    });
                }
                else
                {
                    qShould.Add(new
                    {
                        nested = new
                        {
                            path = "cast",
                            query = new
                            {
                                match = new Dictionary<string, object>
                                {
                                    ["cast.fullName"] = new { query = qs, fuzziness = "AUTO" }
                                }
                            }
                        }
                    });
                }

                // 3) Nested: tags.tagName / tags.slug
                if (qLen <= 2)
                {
                    qShould.Add(new
                    {
                        nested = new
                        {
                            path = "tags",
                            query = new
                            {
                                match_phrase_prefix = new Dictionary<string, object>
                                {
                                    ["tags.tagName"] = new { query = qs, max_expansions = 50 }
                                }
                            }
                        }
                    });
                }
                else
                {
                    // kết hợp match theo tên + term theo slug (nếu mapping slug là keyword)
                    qShould.Add(new
                    {
                        nested = new
                        {
                            path = "tags",
                            query = new
                            {
                                @bool = new
                                {
                                    should = new object[]
                                    {
                                new { match = new Dictionary<string, object> { ["tags.tagName"] = new { query = qs, fuzziness = "AUTO" } } },
                                // fallback: nếu slug là keyword, dùng term; nếu slug là text thì vẫn ổn (term có thể không match -> vẫn còn match theo tagName)
                                new { term  = new Dictionary<string, object> { ["tags.slug"] = qLower! } }
                                    },
                                    minimum_should_match = 1
                                }
                            }
                        }
                    });
                }

                // 4) Heuristic theo loại dữ liệu
                // Năm (4 chữ số)
                if (int.TryParse(qs, out var year) && year >= 1900 && year <= 2100)
                {
                    qShould.Add(new { term = new Dictionary<string, object> { ["year"] = year } });
                }

                // MovieType / Status / Rated (nếu user gõ đúng từ khoá)
                if (qLower is "movie" or "series")
                {
                    qShould.Add(new { term = new Dictionary<string, object> { ["movieType"] = qLower! } });
                }
                if (qLower is "completed" or "ongoing" or "coming_soon")
                {
                    qShould.Add(new { term = new Dictionary<string, object> { ["status"] = qLower! } });
                }
                // Rated (ví dụ: "pg-13", "r")
                if (!string.IsNullOrEmpty(qLower) && (qLower.StartsWith("pg") || qLower is "g" or "r" or "nc-17"))
                {
                    qShould.Add(new { term = new Dictionary<string, object> { ["rated"] = qs! } });
                }
            }

            // build bool query
            var boolDict = new Dictionary<string, object>();
            if (filterMust.Count > 0) boolDict["must"] = filterMust;
            if (qShould.Count > 0)
            {
                boolDict["should"] = qShould;
                boolDict["minimum_should_match"] = 1;
            }
            var query = new { @bool = boolDict };

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
                },
                _source = new[]
                {
            "id","slug","title","year",
            "regionId","regionCode","regionName",
            "tags","popularity","updatedAt"
        }
            };
            using var searchDoc = await PostJsonAsync(http, $"{_moviesIdx}/_search", searchBody, ct);

            // _count (tổng)
            using var countDoc = await PostJsonAsync(http, $"{_moviesIdx}/_count", new { query }, ct);
            var total = countDoc.RootElement.GetProperty("count").GetInt64();

            // Parse hits -> items
            var items = new List<object>();
            var hits = searchDoc.RootElement.GetProperty("hits").GetProperty("hits");
            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var doc = JsonSerializer.Deserialize<MovieDoc>(src, JsonOpts)!; // nhớ bật JsonOpts case-insensitive ở controller

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
                    tags = doc.Tags ?? new List<MovieDoc.TagMini>(),
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
                // Dùng prefix đơn giản trên title (tránh title.auto nếu chưa mapping)
                query = new
                {
                    match_phrase_prefix = new Dictionary<string, object>
                    {
                        ["title"] = new { query = q.Trim(), max_expansions = 50 }
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
                var d = JsonSerializer.Deserialize<MovieDoc>(src, JsonOpts)!; // ✅
                items.Add(new { d.Id, d.Slug, d.Title, d.Year });
            }

            return Ok(items);
        }

        // ===================== SEARCH PERSONS =====================
        [HttpGet("persons")]
        public async Task<IActionResult> SearchPersons([FromQuery] string q, [FromQuery] string? regionCode = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());

            var must = new List<object>
            {
                new
                {
                    multi_match = new
                    {
                        query = q.Trim(),
                        fields = new[] { "fullName^3", "knownFor", "biography" }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(regionCode))
            {
                must.Add(new
                {
                    term = new Dictionary<string, object> { ["regionCode"] = regionCode! }
                });
            }

            var query = new { @bool = new { must } };
            var http = CreateHttp();

            var body = new
            {
                size = 20,
                query,
                _source = new[] { "id", "fullName", "regionId", "regionCode", "regionName", "knownFor" }
            };

            using var doc = await PostJsonAsync(http, $"{_personsIdx}/_search", body, ct);

            var items = new List<object>();
            var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var d = JsonSerializer.Deserialize<PersonDoc>(src, JsonOpts)!; // ✅
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

        // ===================== REINDEX ALL (tiện debug) =====================
        [HttpPost("movies/all")]
        public async Task<IActionResult> ReindexAllMovies(CancellationToken ct)
        {
            var (total, batches) = await _movieIndexService.ReindexAllMoviesAsync(ct);
            return Ok(new { message = "Reindex done", totalIndexed = total, batches });
        }
    }
}
