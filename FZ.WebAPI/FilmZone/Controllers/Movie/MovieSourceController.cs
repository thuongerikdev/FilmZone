using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class MovieSourceController : Controller
    {
        private readonly IMovieSourceService _movieSourceService;
        public MovieSourceController(IMovieSourceService movieSourceService)
        {
            _movieSourceService = movieSourceService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateMovieSource(CreateMovieSourceRequest createMovieSourceRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _movieSourceService.CreateMovieSource(createMovieSourceRequest, ct);
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
        public async Task<IActionResult> UpdateMovieSource(UpdateMovieSourceRequest updateMovieSourceRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _movieSourceService.UpdateMovieSource(updateMovieSourceRequest, ct);
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
        public async Task<IActionResult> DeleteMovieSource(int id, CancellationToken ct)
        {
            try
            {
                var result = await _movieSourceService.DeleteMovieSource(id, ct);
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
        [Authorize(Policy = "ActiveVIP")]
        [HttpGet("/api/movies/{movieId:int}/vip-source")]
        //[HttpGet("{movieId}")]
        public async Task<IActionResult> GetMovieSourcesByMovieId(int movieId, CancellationToken ct)
        {
            try
            {
                var result = await _movieSourceService.GetMovieSourcesByMovieID(movieId, ct);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieSourceById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _movieSourceService.GetMovieSourceByID(id, ct);
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

        [HttpGet("getByMovieId/{movieId}")]
        public async Task<IActionResult> GetMovieSourcesByMovieIdPublic(int movieId, CancellationToken ct)
        {
            try
            {
                var result = await _movieSourceService.GetMovieSourcesByMovieID(movieId, ct);
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
