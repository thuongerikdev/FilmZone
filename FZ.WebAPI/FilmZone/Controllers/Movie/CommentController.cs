using FZ.Constant;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;

        }
        [HttpPost]
        [Authorize(Policy = "CommentCreate")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _commentService.CreateComment(request, ct);
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
        [Authorize(Policy = "CommentUpdateOwn")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _commentService.UpdateComment(request, ct);
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
        [Authorize(Policy = "CommentDeleteOwn")]
        public async Task<IActionResult> DeleteComment(int id, CancellationToken ct)
        {
            try
            {
                var result = await _commentService.DeleteComment(id, ct);
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
        [Authorize(Policy = "CommentRead")]
        public async Task<IActionResult> GetCommentByID(int id, CancellationToken ct)
        {
            try
            {
                var result = await _commentService.GetCommentByID(id, ct);
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
        [HttpGet("{userID}")]
        [Authorize(Policy = "CommentRead")]
        public async Task<IActionResult> GetCommentsByUserID( int userID, CancellationToken ct)
        {
            try
            {
                var result = await _commentService.GetCommentsByUserID(userID, ct);
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
        [HttpGet("{movieID}")]
        [Authorize(Policy = "CommentRead")]
        public async Task<IActionResult> GetCommentsByMovieID(int movieID, CancellationToken ct)
        {
            try
            {
                var result = await _commentService.GetCommentsByMovieID(movieID, ct);
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
