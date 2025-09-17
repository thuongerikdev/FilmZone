using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Archive
{
    public class InternetArchiveS3FileProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "archive-file";

        public InternetArchiveS3FileProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        {
            _httpFactory = httpFactory;
            _hub = hub;
            _cfg = cfg;
        }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (ctx.FileStream == null || ctx.FileSize <= 0)
                return new(false, null, null, null, "No file");

            try
            {
                // 1) Build identifier + target URL
                var prefix = _cfg["Archive:BucketPrefix"] ?? "fz-";
                // Bắt buộc giữ đuôi hợp lệ để IA nhận diện
                var fileName = string.IsNullOrWhiteSpace(ctx.FileName) ? "movie.mp4" : ctx.FileName;
                var identifier = MakeIdentifier(prefix, fileName, DateTime.UtcNow);
                var safeFileName = WebUtility.UrlEncode(fileName);
                var targetUrl = $"https://s3.us.archive.org/{identifier}/{safeFileName}";

                // 2) Headers (ASCII-only)
                var ak = _cfg["Archive:AccessKey"];
                var sk = _cfg["Archive:Secret"];
                if (string.IsNullOrWhiteSpace(ak) || string.IsNullOrWhiteSpace(sk))
                    return new(false, null, null, null, "Archive credentials missing");

                // ⚠️ Chọn collection mà key có quyền: "community" hoặc "opensource_movies" hay collection của bạn
                var collection = _cfg["Archive:Collection"] ?? "community";

                var titleOriginal = Path.GetFileNameWithoutExtension(fileName);
                var titleAscii = HeaderEncoding.ToAsciiHeader(titleOriginal);
                var languageAscii = HeaderEncoding.ToAsciiHeader(ctx.Language ?? "vi");
                var collectionAscii = HeaderEncoding.ToAsciiHeader(collection);

                var req = new HttpRequestMessage(HttpMethod.Put, targetUrl)
                {
                    // tránh 100-continue roundtrip
                    Headers = { ExpectContinue = false }
                };

                // Header chuẩn của IA
                req.Headers.TryAddWithoutValidation("authorization", $"LOW {ak}:{sk}");
                req.Headers.TryAddWithoutValidation("x-archive-auto-make-bucket", "1");
                req.Headers.TryAddWithoutValidation("x-archive-meta-collection", collectionAscii);
                req.Headers.TryAddWithoutValidation("x-archive-meta-mediatype", "movies");
                req.Headers.TryAddWithoutValidation("x-archive-meta-title", titleAscii);
                req.Headers.TryAddWithoutValidation("x-archive-meta-language", languageAscii);
                req.Headers.TryAddWithoutValidation("x-archive-queue-derive", "1"); // 👈 bắt derive để có preview

                // 3) Body + progress + Content-Length + MIME
                if (ctx.FileStream.CanSeek) ctx.FileStream.Seek(0, SeekOrigin.Begin);

                long uploaded = 0;
                var ps = new ProgressStream(ctx.FileStream, adv =>
                {
                    uploaded = adv;
                    var pct = (int)Math.Floor(uploaded * 100.0 / ctx.FileSize);
                    progress.Report(pct);
                    _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                        "upload.progress",
                        new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {uploaded:N0}/{ctx.FileSize:N0}" },
                        ctx.Ct
                    );
                });

                // đoán MIME theo đuôi
                string mime = GuessVideoMime(fileName);

                var content = new StreamContent(ps);
                content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                content.Headers.ContentLength = ctx.FileSize; // ✅ tránh TE: chunked
                req.Content = content;

                var http = _httpFactory.CreateClient("archive"); // bạn đã cấu hình UA, v.v.
                using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(ctx.Ct);
                    if ((int)resp.StatusCode == 403 && body.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
                        return new(false, null, null, null, "Internet Archive: AccessDenied – key không có quyền với collection. Dùng 'community' hoặc xin quyền.");
                    return new(false, null, null, null, $"Archive PUT failed: {(int)resp.StatusCode} {body}");
                }

                // 4) Derive async — trả về details/player url
                var details = $"https://archive.org/details/{identifier}";
                await _hub.Clients.Group(ctx.JobId).SendAsync(
                    "upload.progress",
                    new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Deriving on Archive.org..." },
                    ctx.Ct
                );

                return new(true, identifier, $"/details/{identifier}", details, null);
            }
            catch (OperationCanceledException)
            {
                return new(false, null, null, null, "Canceled");
            }
            catch (Exception ex)
            {
                return new(false, null, null, null, ex.Message);
            }
        }

        private static string GuessVideoMime(string fileName)
        {
            var lower = fileName.ToLowerInvariant();
            if (lower.EndsWith(".mp4")) return "video/mp4";
            if (lower.EndsWith(".webm")) return "video/webm";
            if (lower.EndsWith(".ogv") || lower.EndsWith(".ogg")) return "video/ogg";
            if (lower.EndsWith(".mov")) return "video/quicktime";
            if (lower.EndsWith(".mkv")) return "video/x-matroska";
            return "application/octet-stream";
        }

        private static string MakeIdentifier(string prefix, string name, DateTime now)
        {
            var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(name).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return $"{prefix}{baseName}-{now:yyyyMMddHHmmss}";
        }
    }
}
