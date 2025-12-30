using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserRatingController : Controller
    {
        private readonly IUserRatingService _userRatingService;
        public UserRatingController(IUserRatingService userRatingService)
        {
            _userRatingService = userRatingService;
        }
        [HttpPost]
        [Authorize(Policy = "RatingCreate")]
        public async Task<IActionResult> CreateUserRating([FromBody] CreateUserRatingRequest userRatingRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _userRatingService.CreateUserRating(userRatingRequest, ct);
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
        [Authorize(Policy = "RatingUpdate")]
        public async Task<IActionResult> UpdateUserRating([FromBody] UpdateUserRatingRequest userRatingRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _userRatingService.UpdateUserRating(userRatingRequest, ct);
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
        [Authorize(Policy = "RatingDelete")]
        public async Task<IActionResult> DeleteUserRating(int id, CancellationToken ct)
        {
            try
            {
                var result = await _userRatingService.DeleteUserRating(id, ct);
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
        [Authorize(Policy = "RatingRead")]
        public async Task<IActionResult> GetUserRatingById(int ID, CancellationToken ct)
        {
            try
            {
                var result = await _userRatingService.GetUserRatingByID(ID, ct);
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
        [HttpGet("{userID}")]
        [Authorize(Policy = "RatingRead")]
        public async Task<IActionResult> GetAllUserRatingsByUserId(int userID, CancellationToken ct)
        {
            try
            {
                var result = await _userRatingService.GetUserRatingsByUserID(userID, ct);
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
        [Authorize(Policy = "RatingRead")]
        public async Task<IActionResult> GetAllUserRatingsByMovieId(int movieID, CancellationToken ct)
        {
            try
            {
                var result = await _userRatingService.GetUserRatingsByMovieID(movieID, ct);
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
