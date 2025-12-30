using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class WatchProgressController : Controller
    {
        private readonly IWatchProgressService _watchProgressService;
        public WatchProgressController(IWatchProgressService watchProgressService)
            {
            _watchProgressService = watchProgressService;
        }
        [HttpPost]
        [Authorize(Policy = "ProgressTrack")]
        public async Task<IActionResult> CreateWatchProgress([FromBody] CreateWatchProgressRequest createWatchProgressRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _watchProgressService.CreateWatchProgress(createWatchProgressRequest, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
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
        public async Task<IActionResult> UpdateWatchProgress([FromBody] UpdateWatchProgressRequest updateWatchProgressRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _watchProgressService.UpdateWatchProgress(updateWatchProgressRequest, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
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
        public async Task<IActionResult> DeleteWatchProgress(int id, CancellationToken ct)
        {
            try
            {
                var result = await _watchProgressService.DeleteWatchProgress(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{userId}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetWatchProgressByUserId(int userId, CancellationToken ct)
        {
            try
            {
                var result = await _watchProgressService.GetWatchProgressByUserID(userId, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{ID}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetWatchProgressByID(int ID, CancellationToken ct)
        {
            try
            {
                var result = await _watchProgressService.GetWatchProgressByID(ID, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{movieId}")]
        [Authorize(Policy = "ProgressRead")]
        public async Task<IActionResult> GetWatchProgressByMovieId(int movieId, CancellationToken ct)
        {
            try
            {
                var result = await _watchProgressService.GetWatchProgressByMovieID(movieId, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
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
