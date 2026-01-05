using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class MovieTagController : Controller
    {
        private readonly IMovieTagService _movieTagService;
        public MovieTagController(IMovieTagService movieTagService)
        {
            _movieTagService = movieTagService;
        }
        [HttpPost]
        [Authorize(Policy = "MovieTagManage")]
        public async Task<IActionResult> AddTagToMovie(CreateMoiveTagRequest createMoiveTagRequest , CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.CreateMovieTag(createMoiveTagRequest, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpPut]
        [Authorize(Policy = "MovieTagManage")]
        public async Task<IActionResult> UpdateMovieTag(UpdateMoiveTagRequest updateMovieTagRequest, CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.UpdateMovieTag(updateMovieTagRequest, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = "MovieTagManage")]
        public async Task<IActionResult> DeleteMovieTag(int id, CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.DeleteMovieTag(id, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{tagID}")]

        public async Task<IActionResult> GetMoviesByTag(int tagID, CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.GetMovieTagByID(tagID, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{movieID}")]

        public async Task<IActionResult> GetTagsByMovie(int movieID, CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.GetTagByMovieID(movieID, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("getMovieByTagID")]

        public async Task<IActionResult> GetMoviesByTagIDs ( [FromQuery] List<int> tagID ,CancellationToken ct)
        {
            try
            {
                var result = await _movieTagService.GetMoviesByTagIDs(tagID, ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
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
