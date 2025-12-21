using System.Net.Http.Headers;
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
                    "id", "slug", "title", "year",
                    "regionId", "regionCode", "regionName",
                    "tags", "popularity", "updatedAt",
                    "image",      // ⬅️ Thêm
                    "movieType",   // ⬅️ Thêm
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
                    image = doc.Image,          // ⬅️ Map Image
                    movieType = doc.MovieType,
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
        public async Task<IActionResult> SuggestMovies([FromQuery] string q, [FromQuery] int size = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());

            size = Math.Clamp(size, 1, 20);

            var qs = q.Trim();
            var qLower = qs.ToLowerInvariant();
            var qLen = qs.Length;

            var http = CreateHttp();

            // ===== Build should clauses cho rất nhiều field =====
            var should = new List<object>();

            // 1) Các field cấp document (không nested)
            if (qLen <= 2)
            {
                // prefix cho chuỗi ngắn
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["title"] = new { query = qs, max_expansions = 50 } } });
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["originalTitle"] = new { query = qs, max_expansions = 50 } } });
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["description"] = new { query = qs, max_expansions = 50 } } });
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["regionName"] = new { query = qs, max_expansions = 50 } } });
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["rated"] = new { query = qs, max_expansions = 50 } } });
                // slug nên là keyword + normalizer lowercase → dùng prefix
                should.Add(new { prefix = new Dictionary<string, object> { ["slug"] = new { value = qLower } } });
                // movieType/status (text ngắn) → prefix cũng ổn
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["movieType"] = new { query = qs, max_expansions = 10 } } });
                should.Add(new { match_phrase_prefix = new Dictionary<string, object> { ["status"] = new { query = qs, max_expansions = 10 } } });
            }
            else
            {
                // multi_match + fuzziness cho chuỗi dài
                should.Add(new
                {
                    multi_match = new
                    {
                        query = qs,
                        fields = new[] {
                    "title^6", "originalTitle^4", "description",
                    "regionName^2", "slug^2", "movieType", "status", "rated"
                },
                        fuzziness = "AUTO"
                    }
                });
            }

            // 2) Nested: cast (fullName, characterName)
            if (qLen <= 2)
            {
                should.Add(new
                {
                    nested = new
                    {
                        path = "cast",
                        query = new
                        {
                            @bool = new
                            {
                                should = new object[]
                                {
                            new { match_phrase_prefix = new Dictionary<string, object> { ["cast.fullName"]     = new { query = qs, max_expansions = 50 } } },
                            new { match_phrase_prefix = new Dictionary<string, object> { ["cast.characterName"] = new { query = qs, max_expansions = 50 } } }
                                },
                                minimum_should_match = 1
                            }
                        }
                    }
                });
            }
            else
            {
                should.Add(new
                {
                    nested = new
                    {
                        path = "cast",
                        query = new
                        {
                            @bool = new
                            {
                                should = new object[]
                                {
                            new { match = new Dictionary<string, object> { ["cast.fullName"]     = new { query = qs, fuzziness = "AUTO" } } },
                            new { match = new Dictionary<string, object> { ["cast.characterName"] = new { query = qs, fuzziness = "AUTO" } } }
                                },
                                minimum_should_match = 1
                            }
                        }
                    }
                });
            }

            // 3) Nested: tags (tagName, slug)
            if (qLen <= 2)
            {
                should.Add(new
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
                            new { match_phrase_prefix = new Dictionary<string, object> { ["tags.tagName"] = new { query = qs, max_expansions = 50 } } }
                                },
                                minimum_should_match = 1
                            }
                        }
                    }
                });
                // slug keyword prefix (nếu slug là keyword + lowercase normalizer)
                should.Add(new
                {
                    nested = new
                    {
                        path = "tags",
                        query = new
                        {
                            prefix = new Dictionary<string, object> { ["tags.slug"] = new { value = qLower } }
                        }
                    }
                });
            }
            else
            {
                should.Add(new
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
                            new { term  = new Dictionary<string, object> { ["tags.slug"]    = qLower } } // nếu slug là keyword
                                },
                                minimum_should_match = 1
                            }
                        }
                    }
                });
            }

            // 4) Heuristic: year, rated, movieType, status, regionCode
            if (int.TryParse(qs, out var year) && year >= 1900 && year <= 2100)
                should.Add(new { term = new Dictionary<string, object> { ["year"] = year } });

            if (qLower.StartsWith("pg") || qLower is "g" or "r" or "nc-17")
                should.Add(new { term = new Dictionary<string, object> { ["rated"] = qs } });

            if (qLower is "movie" or "series")
                should.Add(new { term = new Dictionary<string, object> { ["movieType"] = qLower } });

            if (qLower is "completed" or "ongoing" or "coming_soon")
                should.Add(new { term = new Dictionary<string, object> { ["status"] = qLower } });

            // regionCode nếu bạn lưu dạng keyword (ví dụ "US", "VN", "001"...)
            if (qs.Length <= 5)
                should.Add(new { term = new Dictionary<string, object> { ["regionCode"] = qs } });


            // ===== Combine query =====
            var query = new
            {
                @bool = new
                {
                    should,
                    minimum_should_match = 1
                }
            };

            // ===== Gọi _search =====
            var body = new
            {
                size,
                query,
                sort = new object[]
                {
            new Dictionary<string, object> { ["popularity"] = new { order = "desc" } },
            new Dictionary<string, object> { ["updatedAt"]  = new { order = "desc" } }
                },
                // Trả thêm nhiều field cho suggest
                _source = new[]
                {
            "id","slug","title","originalTitle",
            "year","image","releaseDate",
            "regionId","regionCode","regionName",
            "tags","popularity",      // ⬅️ Thêm
            "movieType"
        }
            };

            using var doc = await PostJsonAsync(http, $"{_moviesIdx}/_search", body, ct);

            var items = new List<object>();
            var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");

            foreach (var h in hits.EnumerateArray())
            {
                var src = h.GetProperty("_source").GetRawText();
                var d = JsonSerializer.Deserialize<MovieDoc>(src, JsonOpts)!;

                // chọn title hợp lý để hiển thị
                var displayTitle = string.IsNullOrWhiteSpace(d.Title) ? d.OriginalTitle : d.Title;

                // cắt bớt tags cho suggest (tránh payload nặng)
                var topTags = (d.Tags ?? new List<MovieDoc.TagMini>()).Take(4);

                items.Add(new
                {
                    id = d.Id,
                    slug = d.Slug,
                    title = displayTitle,
                    year = d.Year,
                    image = d.Image,
                    releaseDate = d.ReleaseDate,
                    region = new { id = d.RegionId, code = d.RegionCode, name = d.RegionName },
                    tags = topTags,
                    movieType = d.MovieType,
                    popularity = d.Popularity
                });
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

        [HttpPost("movies/reset-index")]
        public async Task<IActionResult> ResetMoviesIndex(CancellationToken ct)
        {
            // 1. Xóa Index cũ
            await _movieIndexService.DeleteIndexAsync(ct);

            // 2. Tạo lại Index mới (Mapping rỗng)
            await IndexBootstrap.EnsureMoviesIndexAsync(_esBaseUrl, _moviesIdx, _esUser, _esPass, ct);

            // 3. Đổ lại dữ liệu từ DB sang
            var (total, batches) = await _movieIndexService.ReindexAllMoviesAsync(ct);

            return Ok(new
            {
                message = "Index has been reset and re-populated.",
                deletedOldIndex = true,
                totalIndexed = total,
                batches
            });
        }

        // 👇 API 2: Chỉ xóa những thằng thừa (Sync)
        // Dùng cái này nếu muốn giữ lại data cũ, chỉ xóa cái đã mất ở DB
        [HttpPost("movies/sync-orphans")]
        public async Task<IActionResult> SyncOrphanMovies(CancellationToken ct)
        {
            var deletedCount = await _movieIndexService.SyncOrphanMoviesAsync(ct);
            return Ok(new
            {
                message = "Orphan synchronization complete.",
                orphansDeleted = deletedCount
            });
        }
    }
}
