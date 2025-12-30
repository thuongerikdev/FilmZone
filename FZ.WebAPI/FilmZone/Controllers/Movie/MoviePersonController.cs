using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class MoviePersonController : Controller
    {
        private readonly IMoviePersonService _moviePersonService;
        public MoviePersonController(IMoviePersonService moviePersonService)
        {
            _moviePersonService = moviePersonService;
        }
        [HttpPost]
        [Authorize(Policy = "MoviePersonManage")]
        public async Task<IActionResult> AddPersonToMovie( [FromBody]CreateMoviePersonRequest createMoviePersonRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _moviePersonService.CreateMoviePerson( createMoviePersonRequest, ct);
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
        [Authorize(Policy = "MoviePersonManage")]
        public async Task<IActionResult> RemovePersonFromMovie(int id, CancellationToken ct)
        {
            try
            {
                var result = await _moviePersonService.DeleteMoviePerson(id, ct);
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
        [HttpGet("{personID}")]
        [Authorize(Policy = "MoviePersonRead")]
        public async Task<IActionResult> GetMoviesByPerson(int personID, CancellationToken ct)
        {
            try
            {
                var result = await _moviePersonService.GetMoviesByPersonID(personID, ct);
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
        [HttpGet("{movieID}")]
        [Authorize(Policy = "MoviePersonRead")]
        public async Task<IActionResult> GetPersonsByMovie(int movieID, CancellationToken ct)
        {
            try
            {
                var result = await _moviePersonService.GetCreditsByMovieID(movieID, ct);
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
