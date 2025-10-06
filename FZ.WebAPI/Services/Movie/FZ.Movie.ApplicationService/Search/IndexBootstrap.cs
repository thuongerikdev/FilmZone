using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Search
{
    public static class IndexBootstrap
    {
        /// <summary>
        /// Tạo HttpClient có thể kèm Basic Auth (nếu có).
        /// </summary>
        private static HttpClient CreateHttpClient(string baseUrl, string? username = null, string? password = null)
        {
            var http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
            if (!string.IsNullOrWhiteSpace(username) && password != null)
            {
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            return http;
        }

        /// <summary>
        /// Kiểm tra index tồn tại.
        /// </summary>
        private static async Task<bool> IndexExistsAsync(HttpClient http, string indexName, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, indexName);
            using var res = await http.SendAsync(req, ct);
            return (int)res.StatusCode == 200;
        }

        /// <summary>
        /// Gửi PUT tạo index với JSON body.
        /// </summary>
        private static async Task CreateIndexAsync(HttpClient http, string indexName, object body, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(body);
            using var req = new HttpRequestMessage(HttpMethod.Put, indexName)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            using var res = await http.SendAsync(req, ct);
            var resText = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Create index '{indexName}' failed ({(int)res.StatusCode}): {resText}");
            }
        }

        public static async Task EnsureMoviesIndexAsync(
            string baseUrl,
            string indexName,
            string? username = null,
            string? password = null,
            CancellationToken ct = default)
        {
            using var http = CreateHttpClient(baseUrl, username, password);
            if (await IndexExistsAsync(http, indexName, ct)) return;

            var body = new
            {
                settings = new
                {
                    index = new
                    {
                        number_of_shards = 1,
                        number_of_replicas = 1
                    },
                    analysis = new
                    {
                        filter = new
                        {
                            folding = new { type = "asciifolding", preserve_original = false },
                            edge_2_20 = new { type = "edge_ngram", min_gram = 2, max_gram = 20 }
                        },
                        analyzer = new
                        {
                            vi_base = new
                            {
                                type = "custom",
                                tokenizer = "standard",
                                filter = new[] { "lowercase", "folding" }
                            },
                            autocomplete = new
                            {
                                type = "custom",
                                tokenizer = "standard",
                                filter = new[] { "lowercase", "folding", "edge_2_20" }
                            }
                        }
                    }
                },
                mappings = new
                {
                    properties = new
                    {
                        id = new { type = "keyword" },
                        slug = new { type = "keyword" },
                        title = new
                        {
                            type = "text",
                            analyzer = "vi_base",
                            fields = new { auto = new { type = "text", analyzer = "autocomplete" } }
                        },
                        originalTitle = new { type = "text", analyzer = "vi_base" },
                        description = new { type = "text", analyzer = "vi_base" },
                        movieType = new { type = "keyword" },
                        image = new { type = "keyword" },
                        status = new { type = "keyword" },
                        releaseDate = new { type = "date" },

                        durationSeconds = new { type = "integer" },
                        totalSeasons = new { type = "integer" },
                        totalEpisodes = new { type = "integer" },

                        regionId = new { type = "integer" },
                        regionCode = new { type = "keyword" },
                        regionName = new
                        {
                            type = "text",
                            analyzer = "vi_base",
                            fields = new { raw = new { type = "keyword" } }
                        },

                        year = new { type = "integer" },
                        rated = new { type = "keyword" },
                        popularity = new { type = "double" },

                        tags = new
                        {
                            type = "nested",
                            properties = new
                            {
                                tagId = new { type = "integer" },
                                tagName = new
                                {
                                    type = "text",
                                    analyzer = "vi_base",
                                    fields = new { raw = new { type = "keyword" } }
                                },
                                slug = new { type = "keyword" }
                            }
                        },

                        cast = new
                        {
                            type = "nested",
                            properties = new
                            {
                                personId = new { type = "integer" },
                                fullName = new
                                {
                                    type = "text",
                                    analyzer = "vi_base",
                                    fields = new { raw = new { type = "keyword" } }
                                },
                                role = new { type = "keyword" },
                                characterName = new { type = "text", analyzer = "vi_base" },
                                creditOrder = new { type = "integer" }
                            }
                        },

                        updatedAt = new { type = "date" }
                    }
                }
            };

            await CreateIndexAsync(http, indexName, body, ct);
        }

        public static async Task EnsurePersonsIndexAsync(
            string baseUrl,
            string indexName,
            string? username = null,
            string? password = null,
            CancellationToken ct = default)
        {
            using var http = CreateHttpClient(baseUrl, username, password);
            if (await IndexExistsAsync(http, indexName, ct)) return;

            var body = new
            {
                settings = new
                {
                    index = new
                    {
                        number_of_shards = 1,
                        number_of_replicas = 1
                    },
                    analysis = new
                    {
                        filter = new
                        {
                            folding = new { type = "asciifolding", preserve_original = false },
                            edge_2_20 = new { type = "edge_ngram", min_gram = 2, max_gram = 20 }
                        },
                        analyzer = new
                        {
                            vi_base = new
                            {
                                type = "custom",
                                tokenizer = "standard",
                                filter = new[] { "lowercase", "folding" }
                            },
                            autocomplete = new
                            {
                                type = "custom",
                                tokenizer = "standard",
                                filter = new[] { "lowercase", "folding", "edge_2_20" }
                            }
                        }
                    }
                },
                mappings = new
                {
                    properties = new
                    {
                        id = new { type = "keyword" },
                        fullName = new
                        {
                            type = "text",
                            analyzer = "vi_base",
                            fields = new { auto = new { type = "text", analyzer = "autocomplete" } }
                        },
                        knownFor = new { type = "text", analyzer = "vi_base" },

                        regionId = new { type = "integer" },
                        regionCode = new { type = "keyword" },
                        regionName = new
                        {
                            type = "text",
                            analyzer = "vi_base",
                            fields = new { raw = new { type = "keyword" } }
                        },

                        biography = new { type = "text", analyzer = "vi_base" },
                        avatar = new { type = "keyword" },
                        birthDate = new { type = "date" },

                        credits = new
                        {
                            type = "nested",
                            properties = new
                            {
                                movieId = new { type = "integer" },
                                title = new { type = "text", analyzer = "vi_base" },
                                year = new { type = "integer" },
                                role = new { type = "keyword" },
                                characterName = new { type = "text", analyzer = "vi_base" },
                                creditOrder = new { type = "integer" }
                            }
                        },

                        updatedAt = new { type = "date" }
                    }
                }
            };

            await CreateIndexAsync(http, indexName, body, ct);
        }
    }
}
