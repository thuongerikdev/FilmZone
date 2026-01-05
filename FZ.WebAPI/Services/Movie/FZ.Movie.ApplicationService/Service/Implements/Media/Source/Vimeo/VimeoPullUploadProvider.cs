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
    public class VimeoPullUploadProvider : IVideoUploadProvider
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IConfiguration _cfg;

        public string SourceType => "vimeo-link";

        public VimeoPullUploadProvider(IHttpClientFactory httpFactory, IHubContext<UploadHub> hub, IConfiguration cfg)
        {
            _httpFactory = httpFactory;
            _hub = hub;
            _cfg = cfg;
        }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(ctx.LinkUrl))
                return new(false, null, null, null, "No link");

            var token = _cfg["Vimeo:AccessToken"];
            var api = _httpFactory.CreateClient("vimeo-api");
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1) Tạo video qua pull (metadata top-level, không nhét privacy.* vào upload)
            var payload = new
            {
                upload = new { approach = "pull", link = ctx.LinkUrl },
                name = $"Pull {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                privacy = new { view = "unlisted" }
            };
            var resp = await api.PostAsJsonAsync("me/videos", payload, ctx.Ct);
            if (!resp.IsSuccessStatusCode)
                return new(false, null, null, null, $"Create (pull) failed: {(int)resp.StatusCode}");

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ctx.Ct));
            var uri = doc.RootElement.GetProperty("uri").GetString(); // /videos/{id}

            // 2) Thông báo đang fetching/processing
            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.status",
                new { jobId = ctx.JobId, status = "Fetching", message = "Vimeo is pulling the file..." }, ctx.Ct);

            // 3) Poll transcode.status cho đến complete
            string status = "in_progress";
            string? playerLink = null;

            for (int i = 0; i < 240 && status == "in_progress"; i++) // cho pull dư thời gian hơn
            {
                var r = await api.GetAsync($"{uri}?fields=upload.status,transcode.status,link,player_embed_url", ctx.Ct);
                if (!r.IsSuccessStatusCode) break;
                using var st = JsonDocument.Parse(await r.Content.ReadAsStringAsync(ctx.Ct));
                var root = st.RootElement;

                // ưu tiên status transcode
                if (root.TryGetProperty("transcode", out var tr) && tr.TryGetProperty("status", out var ts))
                    status = ts.GetString() ?? "in_progress";

                if (status == "complete")
                {
                    if (root.TryGetProperty("player_embed_url", out var e)) playerLink = e.GetString();
                    else if (root.TryGetProperty("link", out var l)) playerLink = l.GetString();
                    break;
                }

                await _hub.Clients.Group(ctx.JobId).SendAsync("upload.status",
                    new { jobId = ctx.JobId, status = "Processing", message = "Transcoding..." }, ctx.Ct);

                await Task.Delay(TimeSpan.FromSeconds(5), ctx.Ct);
            }

            if (status != "complete")
                return new(false, null, uri, null, $"Transcode not complete ({status})");

            var vid = uri?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return new(true, vid, uri, playerLink ?? (vid != null ? $"https://player.vimeo.com/video/{vid}" : null), null);
        }
    }
}
