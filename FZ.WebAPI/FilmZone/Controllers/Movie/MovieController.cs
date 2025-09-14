using FZ.Constant;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MovieController : Controller
    {
        private readonly IMoviesService _movieService;
        public MovieController(IMoviesService movieService)
        {
            _movieService = movieService;
        }
        // Define your endpoints here, for example:
        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromForm] CreateMoviesRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _movieService.CreateMovie(request, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateMovie([FromForm] UpdateMoviesRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _movieService.UpdateMovie(request, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id, CancellationToken ct)
        {
            try
            {
                var result = await _movieService.DeleteMovie(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _movieService.GetMovieByID(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpGet("gellAll")]
        public async Task<IActionResult> GetAllMovies(CancellationToken ct)
        {
            try
            {
                var result = await _movieService.GetAllMovies(ct);
                if (result.ErrorCode != 200)
                {
                    return BadRequest(ResponseConst.Error<string>(500, result.ErrorMessage));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

    }
}
