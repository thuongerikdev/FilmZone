using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.ApplicationService.Service.Implements.Media.Source;
using FZ.Movie.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace FZ.WebAPI.Controllers.Source
{
    [ApiController]
    [Route("api/upload/youtube")]
    public class YouTubeUploadController : ControllerBase
    {
        private readonly ChannelWriter<UploadWorkItem> _writer;

        public YouTubeUploadController(ChannelWriter<UploadWorkItem> writer) => _writer = writer;

        // YouTubeUploadController.cs
        [HttpPost("file")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest form, CancellationToken ct)
        {
            if (form.File is null || form.File.Length == 0) return BadRequest("No file");

            var jobId = Guid.NewGuid().ToString("N");
            var ctx = new UploadContext(
                JobId: jobId,
                SourceType: "youtube-file",
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

    }
}
