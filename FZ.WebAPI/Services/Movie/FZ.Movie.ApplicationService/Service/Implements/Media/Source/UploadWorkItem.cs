using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using FZ.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media.Source
{
    public class UploadWorkItem
    {
        public UploadContext Ctx { get; init; }
    }



    // using các namespace của bạn
    // using FZ.Movie.ApplicationService.Media;  // nơi chứa IMovieSourceService, IEpisodeSourceService & các DTO Upsert*
    // using FZ.Movie.Web.Hubs;                  // UploadHub
    // using FZ.Movie.Infrastructure.Upload;     // UploadWorkItem, ProviderResolver, IVideoUploadProvider

    public class UploadCoordinator : BackgroundService
    {
        private readonly ChannelReader<UploadWorkItem> _reader;
        private readonly ProviderResolver _resolver;
        private readonly IHubContext<UploadHub> _hub;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UploadCoordinator> _logger;

        public UploadCoordinator(
            ChannelReader<UploadWorkItem> reader,
            ProviderResolver resolver,
            IHubContext<UploadHub> hub,
            IServiceScopeFactory scopeFactory,
            ILogger<UploadCoordinator> logger)
        {
            _reader = reader;
            _resolver = resolver;
            _hub = hub;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in _reader.ReadAllAsync(stoppingToken))
            {
                var ctx = job.Ctx;
                try
                {
                    var provider = _resolver.Resolve(ctx.SourceType);

                    var progress = new Progress<int>(p =>
                        _ = _hub.Clients.Group(ctx.JobId).SendAsync(
                            "upload.progress",
                            new { jobId = ctx.JobId, status = "Uploading", percent = p },
                            ctx.Ct
                        ));

                    var result = await provider.UploadAsync(ctx, progress);
                    if (!result.Success)
                    {
                        await _hub.Clients.Group(ctx.JobId).SendAsync(
                            "upload.error",
                            new { jobId = ctx.JobId, error = result.Error }, ctx.Ct);
                        continue;
                    }

                    var sourceType = ctx.SourceType.Contains("vimeo", StringComparison.OrdinalIgnoreCase) ? "vimeo" :
                                     ctx.SourceType.Contains("youtube", StringComparison.OrdinalIgnoreCase) ? "youtube" :
                                     ctx.SourceType.Contains("archive", StringComparison.OrdinalIgnoreCase) ? "archive" :
                                     ctx.SourceType.ToLowerInvariant();

                    var sourceName = sourceType switch
                    {
                        "vimeo" => "Vimeo",
                        "youtube" => "YouTube",
                        "archive" => "Archive",
                        _ => sourceType
                    };

                    // TẠO SCOPE rồi resolve các service scoped bên trong
                    using var scope = _scopeFactory.CreateScope();
                    var episodeSourceService = scope.ServiceProvider.GetRequiredService<IEpisodeSourceService>();
                    var movieSourceService = scope.ServiceProvider.GetRequiredService<IMovieSourceService>();

                    if (ctx.Scope.Equals("episode", StringComparison.OrdinalIgnoreCase))
                    {
                        var upsertReq = new UpsertEpisodeSourceFromVendorRequest
                        {
                            EpisodeId = ctx.TargetId,
                            SourceName = sourceName,
                            SourceType = sourceType,
                            SourceUrl = result.PlayerUrl!,
                            SourceId = result.VendorVideoId!,
                            Quality = ctx.Quality,
                            Language = ctx.Language,
                            IsVipOnly = ctx.IsVipOnly,
                            IsActive = ctx.IsActive
                        };
                        var resp = await episodeSourceService.UpsertFromVendorAsync(upsertReq, ctx.Ct);
                        if (resp.ErrorCode >= 400)
                            throw new InvalidOperationException(resp.ErrorMessage ?? "Upsert episode source failed");
                    }
                    else
                    {
                        var upsertReq = new UpsertMovieSourceFromVendorRequest
                        {
                            MovieId = ctx.TargetId,
                            SourceName = sourceName,
                            SourceType = sourceType,
                            SourceUrl = result.PlayerUrl!,
                            SourceId = result.VendorVideoId!,
                            Quality = ctx.Quality,
                            Language = ctx.Language,
                            IsVipOnly = ctx.IsVipOnly,
                            IsActive = ctx.IsActive
                        };
                        var resp = await movieSourceService.UpsertFromVendorAsync(upsertReq, ctx.Ct);
                        if (resp.ErrorCode >= 400)
                            throw new InvalidOperationException(resp.ErrorMessage ?? "Upsert movie source failed");
                    }

                    await _hub.Clients.Group(ctx.JobId).SendAsync(
                        "upload.done",
                        new { jobId = ctx.JobId, vendorId = result.VendorVideoId, playerUrl = result.PlayerUrl },
                        ctx.Ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Upload job failed {JobId}", ctx.JobId);
                    await _hub.Clients.Group(ctx.JobId).SendAsync(
                        "upload.error", new { jobId = ctx.JobId, error = ex.Message }, ctx.Ct);
                }
                finally
                {
                    try { ctx.FileStream?.Dispose(); } catch { /* ignore */ }
                    if (!string.IsNullOrWhiteSpace(ctx.TempFilePath))
                    {
                        try { System.IO.File.Delete(ctx.TempFilePath); } catch { /* ignore */ }
                    }
                }
            }
        }
    }

}
