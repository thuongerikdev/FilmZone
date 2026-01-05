using FZ.Constant;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SavedMovieController : Controller
    {
      private readonly ISavedMovieService _savedMovieService;
        public SavedMovieController(ISavedMovieService savedMovieService)
        {
            _savedMovieService = savedMovieService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSavedMovie([FromBody] CreateSavedMovieRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _savedMovieService.CreateSavedMovie(request, ct);
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
        public async Task<IActionResult> UpdateSavedMovie([FromBody] UpdateSavedMovieRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _savedMovieService.UpdateSavedMovie(request, ct);
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
        public async Task<IActionResult> DeleteSavedMovie(int id, CancellationToken ct)
        {
            try
            {
                var result = await _savedMovieService.DeleteSavedMovie(id, ct);
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
        public async Task<IActionResult> GetSavedMovieByID(int id, CancellationToken ct)
        {
            try
            {
                var result = await _savedMovieService.GetSavedMovieByID(id, ct);
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
        public async Task<IActionResult> GetSavedMoviesByUserID(int userId, CancellationToken ct)
        {
            try
            {
                var result = await _savedMovieService.GetSavedMoviesByUserID(userId, ct);
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
        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetSavedMoviesByMovieID(int movieId, CancellationToken ct)
        {
            try
            {
                var result = await _savedMovieService.GetSavedMoviesByMovieID(movieId, ct);
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
