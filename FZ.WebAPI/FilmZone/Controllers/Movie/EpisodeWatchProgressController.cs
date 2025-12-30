using FZ.Constant;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EpisodeWatchProgressController : Controller
    {
        private readonly IEpisodeWatchProgressService _episodeWatchProgressService;
        public EpisodeWatchProgressController(IEpisodeWatchProgressService episodeWatchProgressService)
        {
            _episodeWatchProgressService = episodeWatchProgressService;
        }
        [HttpPost]
        [Authorize(Policy = "ProgressTrack")]
        public async Task<IActionResult> CreateEpisodeWatchProgress([FromBody] CreateEpisodeWatchProgressRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeWatchProgressService.CreateEpisodeWatchProgress(request, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpPut]
        [Authorize(Policy = "ProgressTrack")]
        public async Task<IActionResult> UpdateEpisodeWatchProgress([FromBody] UpdateEpisodeWatchProgressRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeWatchProgressService.UpdateEpisodeWatchProgress(request, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = "ProgressTrack")]
        public async Task<IActionResult> DeleteEpisodeWatchProgress(int id, CancellationToken ct)
        {
            try
            {
                var result = await _episodeWatchProgressService.DeleteEpisodeWatchProgress(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{id}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetEpisodeWatchProgressByID(int id, CancellationToken ct)
        {
            try
            {
                var result = await _episodeWatchProgressService.GetEpisodeWatchProgressByID(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetEpisodeWatchProgressByUserID(int userId, CancellationToken ct)
        {
            try
            {
                var result = await _episodeWatchProgressService.GetEpisodeWatchProgressByUserID(userId, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("episode/{episodeId}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetEpisodeWatchProgressByEpisodeID(int episodeId, CancellationToken ct)
        {
            try
            {
                var result = await _episodeWatchProgressService.GetEpisodeWatchProgressByEpisodeID(episodeId, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

    }
}
