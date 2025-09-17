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

        public string SourceType => "youtube-file";

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

            try
            {
                // đảm bảo stream bắt đầu từ 0 (nếu có thể seek)
                if (ctx.FileStream.CanSeek)
                    ctx.FileStream.Seek(0, SeekOrigin.Begin);

                using var yt = CreateService();

                var defaultTitle = string.IsNullOrWhiteSpace(ctx.FileName)
                    ? $"Upload {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                    : Path.GetFileNameWithoutExtension(ctx.FileName);

                var video = new Video
                {
                    Snippet = new VideoSnippet
                    {
                        Title = defaultTitle,
                        Description = "Uploaded via SDK",
                        Tags = new[] { "api", "upload" },
                        CategoryId = "22" // People & Blogs (tùy bạn)
                    },
                    Status = new VideoStatus { PrivacyStatus = "unlisted" } // public|unlisted|private
                };

                var insert = yt.Videos.Insert(video, "snippet,status", ctx.FileStream, "video/*");
                insert.ChunkSize = 8 * 1024 * 1024; // 8MB

                insert.ProgressChanged += async (IUploadProgress p) =>
                {
                    switch (p.Status)
                    {
                        case UploadStatus.Uploading:
                            var sent = p.BytesSent;
                            var pct = ctx.FileSize > 0 ? (int)Math.Floor(sent * 100.0 / ctx.FileSize) : 0;
                            progress.Report(pct);
                            await _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.progress",
                                new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {sent}/{ctx.FileSize}" },
                                ctx.Ct
                            );
                            break;

                        case UploadStatus.Completed:
                            await _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.progress",
                                new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "YouTube processing..." },
                                ctx.Ct
                            );
                            break;

                        case UploadStatus.Failed:
                            await _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.error",
                                new { jobId = ctx.JobId, error = p.Exception?.Message ?? "Upload failed" },
                                ctx.Ct
                            );
                            break;
                    }
                };

                var result = await insert.UploadAsync(ctx.Ct);

                if (result.Status != UploadStatus.Completed)
                    return new(false, null, null, null, $"Upload failed: {result.Exception?.Message}");

                var videoId = insert.ResponseBody?.Id;
                if (string.IsNullOrEmpty(videoId))
                    return new(false, null, null, null, "Upload finished but no video id");

                // (tuỳ chọn) Poll đến khi xong processing
                var processed = await WaitUntilProcessedAsync(yt, videoId!, ctx);
                if (!processed.ok)
                    return new(false, videoId, null, null, $"Processing status: {processed.status}");

                // phát sự kiện done về FE (nếu bạn đang lắng nghe)
                await _hub.Clients.Group(ctx.JobId).SendAsync(
                    "upload.done",
                    new { jobId = ctx.JobId, vendorId = videoId, playerUrl = $"https://www.youtube.com/watch?v={videoId}" },
                    ctx.Ct
                );

                return new(true, videoId, $"/youtube/{videoId}", $"https://www.youtube.com/watch?v={videoId}", null);
            }
            catch (OperationCanceledException)
            {
                await _hub.Clients.Group(ctx.JobId).SendAsync(
                    "upload.error",
                    new { jobId = ctx.JobId, error = "Canceled" },
                    ctx.Ct
                );
                return new(false, null, null, null, "Canceled");
            }
            catch (Exception ex)
            {
                await _hub.Clients.Group(ctx.JobId).SendAsync(
                    "upload.error",
                    new { jobId = ctx.JobId, error = ex.Message },
                    ctx.Ct
                );
                return new(false, null, null, null, ex.Message);
            }
        }

        private async Task<(bool ok, string status)> WaitUntilProcessedAsync(YouTubeService yt, string videoId, UploadContext ctx)
        {
            // ~20 phút (mỗi 5 giây)
            for (int i = 0; i < 240; i++)
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
