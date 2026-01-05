//using FZ.Movie.ApplicationService.Service.Abtracts;
//using FZ.WebAPI.SignalR;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Youtube
//{
//    public class YouTubeResumableProvider : IVideoUploadProvider
//    {
//        private readonly IHttpClientFactory _httpFactory;
//        private readonly IHubContext<UploadHub> _hub;
//        private readonly IGoogleAccessTokenProvider _tokenProvider;

//        public string SourceType => "youtube-file";

//        public YouTubeResumableProvider(
//            IHttpClientFactory httpFactory,
//            IHubContext<UploadHub> hub,
//            IGoogleAccessTokenProvider tokenProvider)
//        {
//            _httpFactory = httpFactory;
//            _hub = hub;
//            _tokenProvider = tokenProvider;
//        }

//        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
//        {
//            if (ctx.FileStream is null || ctx.FileSize <= 0)
//                return new(false, null, null, null, "No file");

//            var api = _httpFactory.CreateClient();
//            var token = await _tokenProvider.GetAccessTokenAsync(ctx.Ct);
//            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//            // ---------- B1: tạo phiên resumable ----------
//            var initUrl = "https://www.googleapis.com/upload/youtube/v3/videos?uploadType=resumable&part=snippet,status";
//            var meta = new
//            {
//                snippet = new
//                {
//                    title = $"Upload {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
//                    description = "Uploaded via API",
//                    tags = new[] { "api", "upload" }
//                },
//                status = new { privacyStatus = "unlisted" }
//            };

//            var initReq = new HttpRequestMessage(HttpMethod.Post, initUrl);
//            initReq.Content = new StringContent(JsonSerializer.Serialize(meta), Encoding.UTF8, "application/json");

//            var initResp = await api.SendAsync(initReq, ctx.Ct);
//            if (!initResp.IsSuccessStatusCode)
//                return new(false, null, null, null, $"Init session failed: {(int)initResp.StatusCode} {await initResp.Content.ReadAsStringAsync(ctx.Ct)}");

//            if (!initResp.Headers.TryGetValues("Location", out var locVals))
//                return new(false, null, null, null, "No resumable Location header");

//            var uploadUrl = locVals.First();

//            // ---------- B2: PUT chunk ----------
//            long offset = 0;
//            const int chunk = 8 * 1024 * 1024;
//            var buffer = new byte[chunk];

//            await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
//                new { jobId = ctx.JobId, status = "Uploading", percent = 0, text = "Starting..." }, ctx.Ct);

//            var http = _httpFactory.CreateClient();

//            while (offset < ctx.FileSize)
//            {
//                var remaining = (int)Math.Min(chunk, ctx.FileSize - offset);
//                var read = await ctx.FileStream.ReadAsync(buffer.AsMemory(0, remaining), ctx.Ct);
//                if (read <= 0) break;

//                var start = offset;
//                var end = offset + read - 1;

//                async Task<HttpResponseMessage> SendOnceAsync(string bearer)
//                {
//                    var put = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
//                    put.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
//                    put.Content = new ByteArrayContent(buffer, 0, read);
//                    put.Content.Headers.ContentType = new MediaTypeHeaderValue("video/*");
//                    put.Content.Headers.ContentLength = read;
//                    put.Headers.TryAddWithoutValidation("Content-Range", $"bytes {start}-{end}/{ctx.FileSize}");
//                    return await http.SendAsync(put, HttpCompletionOption.ResponseHeadersRead, ctx.Ct);
//                }

//                // gửi lần 1 với token hiện tại
//                var resp = await SendOnceAsync(token);

//                // nếu hết hạn giữa chừng → 401 → lấy token mới và thử lại 1 lần
//                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//                {
//                    resp.Dispose();
//                    token = await _tokenProvider.GetAccessTokenAsync(ctx.Ct); // làm mới
//                    resp = await SendOnceAsync(token);
//                }

//                if ((int)resp.StatusCode == 308)
//                {
//                    if (resp.Headers.TryGetValues("Range", out var ranges))
//                    {
//                        // "bytes=0-8388607"
//                        var last = ranges.First().Split('-').Last();
//                        if (long.TryParse(last, out var lastByte))
//                            offset = lastByte + 1;
//                        else
//                            offset += read;
//                    }
//                    else offset += read;
//                }
//                else if (resp.IsSuccessStatusCode)
//                {
//                    var body = await resp.Content.ReadAsStringAsync(ctx.Ct);
//                    using var doc = string.IsNullOrWhiteSpace(body) ? null : JsonDocument.Parse(body);
//                    var videoId = doc?.RootElement.TryGetProperty("id", out var idProp) == true
//                        ? idProp.GetString()
//                        : null;

//                    if (string.IsNullOrEmpty(videoId))
//                        return new(false, null, null, null, "Upload finished but no video id");

//                    await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
//                        new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "YouTube processing..." }, ctx.Ct);

//                    var processed = await WaitUntilProcessedAsync(api, videoId!, ctx);
//                    if (!processed.ok)
//                        return new(false, videoId, null, null, $"Processing status: {processed.status}");

//                    return new(true, videoId, $"/youtube/{videoId}", $"https://www.youtube.com/watch?v={videoId}", null);
//                }
//                else
//                {
//                    var err = await resp.Content.ReadAsStringAsync(ctx.Ct);
//                    return new(false, null, null, null, $"PUT failed: {(int)resp.StatusCode} {err}");
//                }

//                var pct = (int)Math.Floor(offset * 100.0 / ctx.FileSize);
//                progress.Report(pct);
//                await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
//                    new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {offset}/{ctx.FileSize}" }, ctx.Ct);
//            }

//            return new(false, null, null, null, "Unexpected termination");
//        }

//        private async Task<(bool ok, string status)> WaitUntilProcessedAsync(HttpClient api, string videoId, UploadContext ctx)
//        {
//            // cập nhật token cho api client mỗi vòng để tránh hết hạn (nhẹ nhàng)
//            for (int i = 0; i < 240; i++)
//            {
//                var token = await _tokenProvider.GetAccessTokenAsync(ctx.Ct);
//                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//                var r = await api.GetAsync($"https://www.googleapis.com/youtube/v3/videos?part=processingDetails,status&id={videoId}", ctx.Ct);
//                if (!r.IsSuccessStatusCode) return (false, $"GET status {(int)r.StatusCode}");

//                using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync(ctx.Ct));
//                if (!doc.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
//                    return (false, "not_found");

//                var item = items[0];
//                var processing = item.GetProperty("processingDetails").GetProperty("processingStatus").GetString() ?? "processing";
//                var uploadStatus = item.GetProperty("status").GetProperty("uploadStatus").GetString() ?? "uploaded";

//                if (!string.Equals(processing, "processing", StringComparison.OrdinalIgnoreCase) &&
//                    string.Equals(uploadStatus, "processed", StringComparison.OrdinalIgnoreCase))
//                    return (true, "processed");

//                await Task.Delay(TimeSpan.FromSeconds(5), ctx.Ct);
//            }
//            return (false, "timeout");
//        }
//    }
//}


using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.WebAPI.SignalR;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source.Youtube
{
    public class YouTubeResumableProvider : IVideoUploadProvider
    {
        private readonly IConfiguration _cfg;
        private readonly IHubContext<UploadHub> _hub;

        public string SourceType => "youtube-file"; // giữ nguyên để tương thích

        public YouTubeResumableProvider(IConfiguration cfg, IHubContext<UploadHub> hub)
        {
            _cfg = cfg;
            _hub = hub;
        }

        private YouTubeService CreateService()
        {
            var clientId = _cfg["YouTube:ClientId"];
            var clientSecret = _cfg["YouTube:ClientSecret"];
            var refreshToken = _cfg["YouTube:RefreshToken"];
            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException("YouTube OAuth config missing (ClientId/ClientSecret/RefreshToken).");
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                Scopes = new[] { YouTubeService.Scope.YoutubeUpload },
            });

            var credential = new UserCredential(flow, "FilmZoneUser",
                new TokenResponse { RefreshToken = refreshToken });

            return new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FilmZone"
            });
        }

        public async Task<ProviderResult> UploadAsync(UploadContext ctx, IProgress<int> progress)
        {
            if (ctx.FileStream is null || ctx.FileSize <= 0)
                return new(false, null, null, null, "No file");

            var yt = CreateService();

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = $"Upload {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                    Description = "Uploaded via SDK",
                    Tags = new[] { "api", "upload" },
                    CategoryId = "22" // ví dụ: People & Blogs
                },
                Status = new VideoStatus { PrivacyStatus = "unlisted" } // public|unlisted|private
            };

            // Tạo lệnh resumable upload qua SDK
            var insert = yt.Videos.Insert(video, "snippet,status", ctx.FileStream, "video/*");

            // Chunk size phải là bội số 256KB (ở đây 8MB)
            insert.ChunkSize = 8 * 1024 * 1024;

            // Theo dõi tiến độ
            insert.ProgressChanged += async (IUploadProgress p) =>
            {
                switch (p.Status)
                {
                    case UploadStatus.Uploading:
                        var sent = p.BytesSent;
                        var pct = ctx.FileSize > 0 ? (int)Math.Floor(sent * 100.0 / ctx.FileSize) : 0;
                        progress.Report(pct);
                        await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                            new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {sent}/{ctx.FileSize}" }, ctx.Ct);
                        break;

                    case UploadStatus.Completed:
                        await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                            new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "YouTube processing..." }, ctx.Ct);
                        break;

                    case UploadStatus.Failed:
                        await _hub.Clients.Group(ctx.JobId).SendAsync("upload.progress",
                            new { jobId = ctx.JobId, status = "Failed", percent = 0, text = p.Exception?.Message ?? "Upload failed" }, ctx.Ct);
                        break;
                }
            };

            // Thực thi upload (SDK tự lo refresh token)
            var result = await insert.UploadAsync(ctx.Ct);

            if (result.Status != UploadStatus.Completed)
                return new(false, null, null, null, $"Upload failed: {result.Exception?.Message}");

            var videoId = insert.ResponseBody?.Id;
            if (string.IsNullOrEmpty(videoId))
                return new(false, null, null, null, "Upload finished but no video id");

            // Poll trạng thái xử lý (optional nhưng hữu ích)
            var processed = await WaitUntilProcessedAsync(yt, videoId!, ctx);
            if (!processed.ok)
                return new(false, videoId, null, null, $"Processing status: {processed.status}");

            return new(true, videoId, $"/youtube/{videoId}", $"https://www.youtube.com/watch?v={videoId}", null);
        }

        private async Task<(bool ok, string status)> WaitUntilProcessedAsync(YouTubeService yt, string videoId, UploadContext ctx)
        {
            for (int i = 0; i < 240; i++) // ~20 phút nếu mỗi lần 5s
            {
                var list = yt.Videos.List("processingDetails,status");
                list.Id = videoId;
                var resp = await list.ExecuteAsync(ctx.Ct);

                var item = resp.Items.FirstOrDefault();
                if (item == null) return (false, "not_found");

                var processing = item.ProcessingDetails?.ProcessingStatus ?? "processing";
                var uploadStatus = item.Status?.UploadStatus ?? "uploaded";

                if (!processing.Equals("processing", StringComparison.OrdinalIgnoreCase) &&
                    uploadStatus.Equals("processed", StringComparison.OrdinalIgnoreCase))
                {
                    return (true, "processed");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), ctx.Ct);
            }
            return (false, "timeout");
        }
    }
}
