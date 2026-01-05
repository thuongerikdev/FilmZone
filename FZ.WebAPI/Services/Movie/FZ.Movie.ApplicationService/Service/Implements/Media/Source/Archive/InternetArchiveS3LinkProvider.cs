using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Archive
{
    public class InternetArchiveS3LinkProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "archive-link";

        public InternetArchiveS3LinkProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        { _httpFactory = httpFactory; _hub = hub; _cfg = cfg; }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(ctx.LinkUrl)) return new(false, null, null, null, "No link");

            // 1) Tải stream từ link (server acts as proxy)
            var src = _httpFactory.CreateClient();
            using var res = await src.GetAsync(ctx.LinkUrl, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
            if (!res.IsSuccessStatusCode) return new(false, null, null, null, $"Fetch link failed: {(int)res.StatusCode}");

            var total = res.Content.Headers.ContentLength ?? 0;
            var fileName = GuessFileName(ctx.LinkUrl, res) ?? "video.mp4";

            // 2) Build identifier + target URL
            var prefix = _cfg["Archive:BucketPrefix"] ?? "fz-";
            var identifier = MakeIdentifier(prefix, fileName, DateTime.UtcNow);
            var safeFileName = WebUtility.UrlEncode(fileName);
            var targetUrl = $"https://s3.us.archive.org/{identifier}/{safeFileName}";

            // 3) Compose headers
            var ak = _cfg["Archive:AccessKey"];
            var sk = _cfg["Archive:Secret"];
            if (string.IsNullOrWhiteSpace(ak) || string.IsNullOrWhiteSpace(sk))
                return new(false, null, null, null, "Archive credentials missing");

            var collection = _cfg["Archive:Collection"] ?? "opensource_movies";
            var title = Path.GetFileNameWithoutExtension(fileName);

            var req = new HttpRequestMessage(HttpMethod.Put, targetUrl);
            req.Headers.TryAddWithoutValidation("authorization", $"LOW {ak}:{sk}");           // :contentReference[oaicite:12]{index=12}
            req.Headers.TryAddWithoutValidation("x-amz-auto-make-bucket", "1");              // :contentReference[oaicite:13]{index=13}
            req.Headers.TryAddWithoutValidation("x-archive-meta01-collection", collection);  // :contentReference[oaicite:14]{index=14}
            req.Headers.TryAddWithoutValidation("x-archive-meta-mediatype", "movies");       // :contentReference[oaicite:15]{index=15}
            req.Headers.TryAddWithoutValidation("x-archive-meta-title", title);              // :contentReference[oaicite:16]{index=16}
            req.Headers.TryAddWithoutValidation("x-archive-meta-language", ctx.Language ?? "vi");
            if (total > 0) req.Headers.TryAddWithoutValidation("x-archive-size-hint", total.ToString()); // :contentReference[oaicite:17]{index=17}

            // 4) Stream sang IA với progress
            long uploaded = 0;
            var srcStream = await res.Content.ReadAsStreamAsync(ctx.Ct);
            var ps = new ProgressStream(srcStream, adv =>
            {
                uploaded = adv;
                if (total > 0)
                {
                    var pct = (int)Math.Floor(uploaded * 100.0 / total);
                    progress.Report(pct);
                    _ = _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                        new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {uploaded}/{total}" }, ctx.Ct);
                }
                else
                {
                    _ = _hub.Clients.Group(ctx.JobId).SendAsync("upload.status",
                        new { jobId = ctx.JobId, status = "Uploading", message = $"Uploaded {uploaded} bytes" }, ctx.Ct);
                }
            });

            var content = new StreamContent(ps);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            if (total > 0) content.Headers.ContentLength = total;
            req.Content = content;

            var http = _httpFactory.CreateClient();
            var put = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
            if (!put.IsSuccessStatusCode)
            {
                var body = await put.Content.ReadAsStringAsync(ctx.Ct);
                return new(false, null, null, null, $"Archive PUT failed: {(int)put.StatusCode} {body}");
            }

            var details = $"https://archive.org/details/{identifier}";
            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Deriving on Archive.org..." }, ctx.Ct);

            return new(true, identifier, $"/details/{identifier}", details, null);
        }

        private static string? GuessFileName(string url, HttpResponseMessage res)
        {
            // Try Content-Disposition
            if (res.Content.Headers.ContentDisposition?.FileNameStar is { Length: > 0 } star) return star;
            if (res.Content.Headers.ContentDisposition?.FileName is { Length: > 0 } fn) return fn.Trim('\"');

            // Fallback to URL path
            var path = new Uri(url).AbsolutePath;
            var last = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return string.IsNullOrWhiteSpace(last) ? null : WebUtility.UrlDecode(last);
        }

        private static string MakeIdentifier(string prefix, string name, DateTime now)
        {
            var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(name).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return $"{prefix}{baseName}-{now:yyyyMMddHHmmss}";
        }
    }
}
