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

                // --- SỬA ĐỔI 1: Đảm bảo tên file luôn có đuôi video ---
                var rawFileName = string.IsNullOrWhiteSpace(ctx.FileName) ? "movie.mp4" : ctx.FileName;
                var fileName = EnsureVideoExtension(rawFileName);
                // ------------------------------------------------------

                // Lưu ý: DateTime.UtcNow nên format cố định để tránh lỗi identifier
                var identifier = MakeIdentifier(prefix, fileName, DateTime.UtcNow);

                // Mã hóa tên file để dùng trên URL
                var safeFileName = WebUtility.UrlEncode(fileName);

                var targetUrl = $"https://s3.us.archive.org/{identifier}/{safeFileName}";

                // 2) Headers (ASCII-only)
                var ak = _cfg["Archive:AccessKey"];
                var sk = _cfg["Archive:Secret"];
                if (string.IsNullOrWhiteSpace(ak) || string.IsNullOrWhiteSpace(sk))
                    return new(false, null, null, null, "Archive credentials missing");

                var collection = _cfg["Archive:Collection"] ?? "community";

                var titleOriginal = Path.GetFileNameWithoutExtension(fileName);
                var titleAscii = HeaderEncoding.ToAsciiHeader(titleOriginal);
                var languageAscii = HeaderEncoding.ToAsciiHeader(ctx.Language ?? "vi");
                var collectionAscii = HeaderEncoding.ToAsciiHeader(collection);

                var req = new HttpRequestMessage(HttpMethod.Put, targetUrl)
                {
                    Headers = { ExpectContinue = false }
                };

                req.Headers.TryAddWithoutValidation("authorization", $"LOW {ak}:{sk}");
                req.Headers.TryAddWithoutValidation("x-archive-auto-make-bucket", "1");
                req.Headers.TryAddWithoutValidation("x-archive-meta-collection", collectionAscii);
                req.Headers.TryAddWithoutValidation("x-archive-meta-mediatype", "movies");
                req.Headers.TryAddWithoutValidation("x-archive-meta-title", titleAscii);
                req.Headers.TryAddWithoutValidation("x-archive-meta-language", languageAscii);
                req.Headers.TryAddWithoutValidation("x-archive-queue-derive", "1");

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

                string mime = GuessVideoMime(fileName);

                var content = new StreamContent(ps);
                content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                content.Headers.ContentLength = ctx.FileSize;
                req.Content = content;

                var http = _httpFactory.CreateClient("archive");
                using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(ctx.Ct);
                    if ((int)resp.StatusCode == 403 && body.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
                        return new(false, null, null, null, "Internet Archive: AccessDenied – key không có quyền với collection. Dùng 'community' hoặc xin quyền.");
                    return new(false, null, null, null, $"Archive PUT failed: {(int)resp.StatusCode} {body}");
                }

                // 4) Derive async
                await _hub.Clients.Group(ctx.JobId).SendAsync(
                    "upload.progress",
                    new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Deriving on Archive.org..." },
                    ctx.Ct
                );

                // --- SỬA ĐỔI 2: Tạo link download trực tiếp ---
                var detailsLink = $"https://archive.org/details/{identifier}";
                var downloadLink = $"https://archive.org/download/{identifier}/{safeFileName}";

                // Tham số thứ 4 là PlayerUrl => Trả về downloadLink (.mp4)
                return new(true, identifier, $"/details/{identifier}", downloadLink, null);
                // ----------------------------------------------
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

        // Hàm helper: Đảm bảo tên file có đuôi video
        private static string EnsureVideoExtension(string fileName)
        {
            var validExtensions = new[] { ".mp4", ".mkv", ".webm", ".avi", ".mov", ".flv", ".wmv", ".m4v" };
            var ext = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(ext) && validExtensions.Contains(ext.ToLowerInvariant()))
            {
                return fileName;
            }
            // Mặc định ép về .mp4 nếu không có đuôi hoặc đuôi lạ
            return $"{fileName}.mp4";
        }

        private static string MakeIdentifier(string prefix, string name, DateTime now)
        {
            var safeName = Path.GetFileNameWithoutExtension(name);
            var baseName = Regex.Replace(safeName.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

            // Giới hạn độ dài để tránh ID quá dài
            if (baseName.Length > 50) baseName = baseName.Substring(0, 50);

            return $"{prefix}{baseName}-{now:yyyyMMddHHmmss}";
        }
    }
}