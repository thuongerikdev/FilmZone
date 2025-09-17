using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Archive
{
    public class InternetArchiveS3LinkProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "archive-link";

        public InternetArchiveS3LinkProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        {
            _httpFactory = httpFactory;
            _hub = hub;
            _cfg = cfg;
        }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(ctx.LinkUrl))
                return new(false, null, null, null, "No link");

            string? tempPath = null;
            try
            {
                // 1) tải nguồn
                var src = _httpFactory.CreateClient();
                using var res = await src.GetAsync(ctx.LinkUrl, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
                if (!res.IsSuccessStatusCode)
                    return new(false, null, null, null, $"Fetch link failed: {(int)res.StatusCode}");

                var total = res.Content.Headers.ContentLength ?? 0;
                var fileName = GuessFileName(ctx.LinkUrl, res) ?? "video.mp4";

                // nếu không biết length → buffer tạm để có Content-Length
                Stream uploadStream;
                long uploadLength;
                if (total <= 0)
                {
                    tempPath = Path.Combine(Path.GetTempPath(), $"ia_buf_{Guid.NewGuid():N}.bin");
                    await _hub.Clients.Group(ctx.JobId).SendAsync(
                        "upload.progress",
                        new { jobId = ctx.JobId, status = "Buffering", percent = 0, text = "Buffering to temp file..." },
                        ctx.Ct
                    );

                    await using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true))
                    {
                        var srcStream = await res.Content.ReadAsStreamAsync(ctx.Ct);
                        var buffer = new byte[1024 * 1024];
                        long copied = 0;
                        int read;
                        while ((read = await srcStream.ReadAsync(buffer.AsMemory(0, buffer.Length), ctx.Ct)) > 0)
                        {
                            await fs.WriteAsync(buffer.AsMemory(0, read), ctx.Ct);
                            copied += read;
                            _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.status",
                                new { jobId = ctx.JobId, status = "Buffering", message = $"Buffered {copied:N0} bytes" },
                                ctx.Ct
                            );
                        }
                    }

                    uploadStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
                    uploadLength = new FileInfo(tempPath).Length;
                }
                else
                {
                    uploadStream = await res.Content.ReadAsStreamAsync(ctx.Ct);
                    uploadLength = total;
                }

                await using (uploadStream)
                {
                    // 2) Build identifier + target URL
                    var prefix = _cfg["Archive:BucketPrefix"] ?? "fz-";
                    var identifier = MakeIdentifier(prefix, fileName, DateTime.UtcNow);
                    var safeFileName = WebUtility.UrlEncode(fileName);
                    var targetUrl = $"https://s3.us.archive.org/{identifier}/{safeFileName}";

                    // 3) Headers
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

                    if (uploadStream.CanSeek) uploadStream.Seek(0, SeekOrigin.Begin);

                    long uploaded = 0;
                    var ps = new ProgressStream(uploadStream, adv =>
                    {
                        uploaded = adv;
                        var pct = uploadLength > 0 ? (int)Math.Floor(uploaded * 100.0 / uploadLength) : 0;
                        progress.Report(pct);
                        _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                            "upload.progress",
                            new
                            {
                                jobId = ctx.JobId,
                                status = "Uploading",
                                percent = pct,
                                text = uploadLength > 0
                                    ? $"Uploaded {uploaded:N0}/{uploadLength:N0}"
                                    : $"Uploaded {uploaded:N0} bytes"
                            },
                            ctx.Ct
                        );
                    });

                    // MIME theo đuôi
                    string mime = GuessVideoMime(fileName);

                    var content = new StreamContent(ps);
                    content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                    content.Headers.ContentLength = uploadLength; // ✅ BẮT BUỘC
                    req.Content = content;

                    var http = _httpFactory.CreateClient("archive");
                    using var put = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
                    if (!put.IsSuccessStatusCode)
                    {
                        var body = await put.Content.ReadAsStringAsync(ctx.Ct);
                        if ((int)put.StatusCode == 403 && body.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
                            return new(false, null, null, null, "Internet Archive: AccessDenied – key không có quyền với collection. Dùng 'community' hoặc xin quyền.");
                        return new(false, null, null, null, $"Archive PUT failed: {(int)put.StatusCode} {body}");
                    }

                    var details = $"https://archive.org/details/{identifier}";
                    await _hub.Clients.Group(ctx.JobId).SendAsync(
                        "upload.progress",
                        new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Deriving on Archive.org..." },
                        ctx.Ct
                    );

                    return new(true, identifier, $"/details/{identifier}", details, null);
                }
            }
            catch (OperationCanceledException)
            {
                return new(false, null, null, null, "Canceled");
            }
            catch (Exception ex)
            {
                return new(false, null, null, null, ex.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* ignore */ }
                }
            }
        }

        private static string? GuessFileName(string url, HttpResponseMessage res)
        {
            if (res.Content.Headers.ContentDisposition?.FileNameStar is { Length: > 0 } star) return star;
            if (res.Content.Headers.ContentDisposition?.FileName is { Length: > 0 } fn) return fn.Trim('\"');

            var path = new Uri(url).AbsolutePath;
            var last = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return string.IsNullOrWhiteSpace(last) ? null : WebUtility.UrlDecode(last);
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
