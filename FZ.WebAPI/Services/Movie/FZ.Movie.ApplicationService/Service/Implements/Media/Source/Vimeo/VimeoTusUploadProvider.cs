using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Vimeo
{
    public class VimeoTusUploadProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "vimeo-file";

        public VimeoTusUploadProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        {
            _httpFactory = httpFactory;
            _hub = hub;
            _cfg = cfg;
        }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (ctx.FileStream is null || ctx.FileSize <= 0)
                return new(false, null, null, null, "No file");

            var token = _cfg["Vimeo:AccessToken"];
            var api = _httpFactory.CreateClient("vimeo-api");
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1) Create video (tus)
            var createPayload = new
            {
                upload = new { approach = "tus", size = ctx.FileSize },
                name = $"Upload {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                privacy = new { view = "unlisted" } // tuỳ chỉnh
            };
            var createResp = await api.PostAsJsonAsync("me/videos", createPayload, ctx.Ct);
            if (!createResp.IsSuccessStatusCode)
                return new(false, null, null, null, $"Create failed: {(int)createResp.StatusCode}");

            using var createDoc = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync(ctx.Ct));
            var root = createDoc.RootElement;
            var uri = root.GetProperty("uri").GetString(); // /videos/{id}
            var uploadLink = root.GetProperty("upload").GetProperty("upload_link").GetString();

            // 2) PATCH chunks theo tus
            long offset = 0;
            const int chunk = 8 * 1024 * 1024;
            var http = _httpFactory.CreateClient();
            var buffer = new byte[chunk];

            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                new { jobId = ctx.JobId, status = "Uploading", percent = 0, text = "Starting..." }, ctx.Ct);

            while (true)
            {
                var read = await ctx.FileStream.ReadAsync(buffer.AsMemory(0, chunk), ctx.Ct);
                if (read <= 0) break;

                using var content = new ByteArrayContent(buffer, 0, read);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");

                var req = new HttpRequestMessage(new HttpMethod("PATCH"), uploadLink);
                req.Content = content;
                req.Headers.TryAddWithoutValidation("Tus-Resumable", "1.0.0");
                req.Headers.TryAddWithoutValidation("Upload-Offset", offset.ToString());

                var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
                if (!resp.IsSuccessStatusCode)
                    return new(false, null, uri, null, $"PATCH failed at {offset}: {(int)resp.StatusCode}");

                if (resp.Headers.TryGetValues("Upload-Offset", out var vals) &&
                    long.TryParse(vals.FirstOrDefault(), out var newOffset))
                    offset = newOffset;
                else
                    offset += read;

                var pct = (int)Math.Floor(offset * 100.0 / ctx.FileSize);
                progress.Report(pct);
                await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                    new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {offset}/{ctx.FileSize}" }, ctx.Ct);
            }

            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "Transcoding..." }, ctx.Ct);

            // 3) Poll transcode.status
            string status = "in_progress";
            string? playerLink = null;
            for (int i = 0; i < 120 && status == "in_progress"; i++)
            {
                var r = await api.GetAsync($"{uri}?fields=transcode.status,link,player_embed_url", ctx.Ct);
                if (!r.IsSuccessStatusCode) break;
                using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync(ctx.Ct));
                status = doc.RootElement.GetProperty("transcode").GetProperty("status").GetString() ?? "in_progress";

                if (status == "complete")
                {
                    if (doc.RootElement.TryGetProperty("player_embed_url", out var e)) playerLink = e.GetString();
                    else if (doc.RootElement.TryGetProperty("link", out var l)) playerLink = l.GetString();
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(5), ctx.Ct);
            }

            if (status != "complete")
                return new(false, null, uri, null, $"Transcode not complete ({status})");

            var vid = uri?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return new(true, vid, uri, playerLink ?? (vid != null ? $"https://player.vimeo.com/video/{vid}" : null), null);
        }
    }
}
