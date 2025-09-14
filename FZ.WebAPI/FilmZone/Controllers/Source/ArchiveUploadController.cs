using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source;
using FZ.Movie.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace FZ.WebAPI.Controllers.Source
{
    [ApiController]
    [Route("api/upload/archive")]
    public class ArchiveUploadController : ControllerBase
    {
        private readonly ChannelWriter<UploadWorkItem> _writer;
        public ArchiveUploadController(ChannelWriter<UploadWorkItem> writer) => _writer = writer;

        // ArchiveUploadController.cs
        [HttpPost("file")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest form, CancellationToken ct)
        {
            if (form.File is null || form.File.Length == 0) return BadRequest("No file");

            var jobId = Guid.NewGuid().ToString("N");
            var ctx = new UploadContext(
                JobId: jobId,
                SourceType: "archive-file",
                Scope: form.Scope,
                TargetId: form.TargetId,
                Quality: form.Quality ?? "1080p",
                Language: form.Language ?? "vi",
                IsVipOnly: form.IsVipOnly,
                IsActive: form.IsActive,
                FileStream: form.File.OpenReadStream(),
                FileSize: form.File.Length,
                LinkUrl: null,
                FileName: form.File.FileName,            // <-- truyền tên file
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
                SourceType: "archive-link",
                Scope: body.Scope,
                TargetId: body.TargetId,
                Quality: body.Quality ?? "1080p",
                Language: body.Language ?? "vi",
                IsVipOnly: body.IsVipOnly,
                IsActive: body.IsActive,
                FileStream: null,
                FileSize: 0,
                LinkUrl: body.LinkUrl,
                FileName: null,                           // <-- không có file local
                Ct: ct
            );
            await _writer.WriteAsync(new UploadWorkItem { Ctx = ctx }, ct);
            return Ok(new { jobId });
        }

    }
}
