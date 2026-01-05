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
    public class InternetArchiveS3FileProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "archive-file";

        public InternetArchiveS3FileProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        { _httpFactory = httpFactory; _hub = hub; _cfg = cfg; }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (ctx.FileStream == null || ctx.FileSize <= 0) return new(false, null, null, null, "No file");

            // 1) Build identifier + target URL
            var prefix = _cfg["Archive:BucketPrefix"] ?? "fz-";
            var identifier = MakeIdentifier(prefix, ctx.FileName ?? "movie", DateTime.UtcNow);
            var safeFileName = WebUtility.UrlEncode(ctx.FileName ?? "movie.mp4");
            var targetUrl = $"https://s3.us.archive.org/{identifier}/{safeFileName}";

            // 2) Compose headers: LOW auth + auto make bucket + metadata
            var ak = _cfg["Archive:AccessKey"];
            var sk = _cfg["Archive:Secret"];
            if (string.IsNullOrWhiteSpace(ak) || string.IsNullOrWhiteSpace(sk))
                return new(false, null, null, null, "Archive credentials missing");

            var collection = _cfg["Archive:Collection"] ?? "opensource_movies"; // hoặc "community"
            var title = Path.GetFileNameWithoutExtension(ctx.FileName) ?? "Untitled";

            var req = new HttpRequestMessage(HttpMethod.Put, targetUrl);
            req.Headers.TryAddWithoutValidation("authorization", $"LOW {ak}:{sk}");            // :contentReference[oaicite:2]{index=2}
            req.Headers.TryAddWithoutValidation("x-amz-auto-make-bucket", "1");               // :contentReference[oaicite:3]{index=3}
            req.Headers.TryAddWithoutValidation("x-archive-meta01-collection", collection);   // :contentReference[oaicite:4]{index=4}
            req.Headers.TryAddWithoutValidation("x-archive-meta-mediatype", "movies");        // video player on details page :contentReference[oaicite:5]{index=5}
            req.Headers.TryAddWithoutValidation("x-archive-meta-title", title);               // :contentReference[oaicite:6]{index=6}
            req.Headers.TryAddWithoutValidation("x-archive-meta-language", ctx.Language ?? "vi");
            req.Headers.TryAddWithoutValidation("x-archive-size-hint", ctx.FileSize.ToString()); // :contentReference[oaicite:7]{index=7}

            // 3) Send streaming body with progress
            long uploaded = 0;
            var ps = new ProgressStream(ctx.FileStream, adv =>
            {
                uploaded = adv;
                var pct = (int)Math.Floor(uploaded * 100.0 / ctx.FileSize);
                progress.Report(pct);
                _ = _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                    new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {uploaded}/{ctx.FileSize}" }, ctx.Ct);
            });
            var content = new StreamContent(ps);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            req.Content = content;

            var http = _httpFactory.CreateClient();
            var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ctx.Ct);
                return new(false, null, null, null, $"Archive PUT failed: {(int)resp.StatusCode} {body}");
            }

            // 4) Derive sẽ chạy async — return details/player url
            var details = $"https://archive.org/details/{identifier}";
            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Deriving on Archive.org..." }, ctx.Ct);

            return new(true, identifier, $"/details/{identifier}", details, null);
        }

        private static string MakeIdentifier(string prefix, string name, DateTime now)
        {
            var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(name).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return $"{prefix}{baseName}-{now:yyyyMMddHHmmss}";
        }
    }
}
