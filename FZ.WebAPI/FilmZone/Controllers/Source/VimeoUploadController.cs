﻿using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source;
using FZ.Movie.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Channels;

namespace FZ.WebAPI.Controllers.Source
{
    [ApiController]
    [Route("api/upload/vimeo")]
    public class VimeoUploadController : ControllerBase
    {
        private readonly ChannelWriter<UploadWorkItem> _writer;
        public VimeoUploadController(ChannelWriter<UploadWorkItem> writer) => _writer = writer;

        [HttpPost("file")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(
            MultipartBodyLengthLimit = long.MaxValue,
            ValueLengthLimit = int.MaxValue,
            MultipartHeadersLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest form, CancellationToken ct)
        {
            if (form.File is null || form.File.Length == 0) return BadRequest("No file");

            var jobId = Guid.NewGuid().ToString("N");
            var safeName = Path.GetFileName(form.File.FileName);
            var tempPath = Path.Combine(Path.GetTempPath(), $"fz_vimeo_{jobId}_{safeName}");

            await using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1_048_576, useAsync: true))
            {
                await form.File.CopyToAsync(fs, ct);
            }

            var read = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1_048_576, useAsync: true);

            var ctx = new UploadContext(
                JobId: jobId,
                SourceType: "vimeo-file",
                Scope: form.Scope,
                TargetId: form.TargetId,
                Quality: form.Quality ?? "1080p",
                Language: form.Language ?? "vi",
                IsVipOnly: form.IsVipOnly,
                IsActive: form.IsActive,
                FileStream: read,
                FileSize: read.Length,
                LinkUrl: null,
                FileName: form.File.FileName,
                TempFilePath: tempPath,
                Ct: ct
            );

            await _writer.WriteAsync(new UploadWorkItem { Ctx = ctx }, ct);
            return Ok(new { jobId });
        }

        [HttpPost("link")]
        public async Task<IActionResult> UploadLink([FromBody] UploadLinkRequest body, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(body.LinkUrl)) return BadRequest("No link");

            var jobId = Guid.NewGuid().ToString("N");
            var ctx = new UploadContext(
                JobId: jobId,
                SourceType: "vimeo-link",
                Scope: body.Scope,
                TargetId: body.TargetId,
                Quality: body.Quality ?? "1080p",
                Language: body.Language ?? "vi",
                IsVipOnly: body.IsVipOnly,
                IsActive: body.IsActive,
                FileStream: null,
                FileSize: 0,
                LinkUrl: body.LinkUrl,
                FileName: null,
                TempFilePath: null,
                Ct: ct
            );

            await _writer.WriteAsync(new UploadWorkItem { Ctx = ctx }, ct);
            return Ok(new { jobId });
        }
    }
}
