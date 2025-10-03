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
        { _cfg = cfg; _hub = hub; }

        private async Task<YouTubeService> CreateServiceAsync()
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
                Scopes = new[]
                 {
                    YouTubeService.Scope.YoutubeUpload,
                    YouTubeService.Scope.YoutubeReadonly // hoặc YouTubeService.Scope.Youtube
                },
            });

            var credential = new UserCredential(flow, "FilmZoneUser",
                new TokenResponse { RefreshToken = refreshToken });
            var scopes = credential.Token.Scope;

            Console.WriteLine("Scopes: " + scopes);

            // Refresh ngay để chắc chắn có AccessToken
            await credential.RefreshTokenAsync(CancellationToken.None);

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
                if (ctx.FileStream.CanSeek) ctx.FileStream.Seek(0, SeekOrigin.Begin);
                using var yt = await CreateServiceAsync();

                var defaultTitle = string.IsNullOrWhiteSpace(ctx.FileName)
                    ? $"Upload {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                    : Path.GetFileNameWithoutExtension(ctx.FileName);

                var video = new Video
                {
                    Snippet = new VideoSnippet
                    {
                        Title = defaultTitle,
                        Description = "Uploaded via API",
                        Tags = new[] { "api", "upload" },
                        CategoryId = "22"
                    },
                    Status = new VideoStatus { PrivacyStatus = "unlisted" } // public|unlisted|private
                };

                var mime = GuessVideoMime(ctx.FileName ?? "video.mp4");

                // OPTION A: dùng ProgressChanged của SDK (khuyên dùng)
                var insert = yt.Videos.Insert(video, "snippet,status", ctx.FileStream, mime);
                insert.ChunkSize = 2 * 1024 * 1024; // 2MB để progress mượt

                insert.ProgressChanged += p =>
                {
                    switch (p.Status)
                    {
                        case UploadStatus.Uploading:
                            var sent = p.BytesSent;
                            var pct = ctx.FileSize > 0 ? (int)Math.Floor(sent * 100.0 / ctx.FileSize) : 0;
                            progress.Report(pct);
                            _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.progress",
                                new { jobId = ctx.JobId, status = "Uploading", percent = pct, text = $"Uploaded {sent:N0}/{ctx.FileSize:N0}" },
                                ctx.Ct
                            );
                            break;

                        case UploadStatus.Completed:
                            _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                                "upload.progress",
                                new { jobId = ctx.JobId, status = "Processing", percent = 100, text = "YouTube processing..." },
                                ctx.Ct
                            );
                            break;

                        case UploadStatus.Failed:
                            _ = _hub.Clients.Group(ctx.JobId).SendAsync(
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

                // Poll đợi processed
                var processed = await WaitUntilProcessedAsync(yt, videoId!, ctx);
                if (!processed.ok)
                    return new(false, videoId, null, null, $"Processing status: {processed.status}");

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
            for (int i = 0; i < 240; i++) // ~20 phút
            {
                var list = yt.Videos.List("processingDetails,status");
                list.Id = videoId;
                var resp = await list.ExecuteAsync(ctx.Ct);
                var item = resp.Items.FirstOrDefault();
                if (item == null) return (false, "not_found");

                var processing = item.ProcessingDetails?.ProcessingStatus ?? "processing"; // processing|succeeded|failed|terminated
                var uploadStatus = item.Status?.UploadStatus ?? "uploaded";               // uploaded|processed|failed

                if (processing.Equals("succeeded", StringComparison.OrdinalIgnoreCase) &&
                    uploadStatus.Equals("processed", StringComparison.OrdinalIgnoreCase))
                {
                    return (true, "processed");
                }
                if (processing.Equals("failed", StringComparison.OrdinalIgnoreCase) ||
                    processing.Equals("terminated", StringComparison.OrdinalIgnoreCase) ||
                    uploadStatus.Equals("failed", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"{processing}/{uploadStatus}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), ctx.Ct);
            }
            return (false, "timeout");
        }

        private static string GuessVideoMime(string fileName)
        {
            var lower = fileName.ToLowerInvariant();
            if (lower.EndsWith(".mp4")) return "video/mp4";
            if (lower.EndsWith(".webm")) return "video/webm";
            if (lower.EndsWith(".mov")) return "video/quicktime";
            if (lower.EndsWith(".mkv")) return "video/x-matroska";
            if (lower.EndsWith(".ogv") || lower.EndsWith(".ogg")) return "video/ogg";
            return "video/*";
        }
    }
}
