using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MovieSubTitleController : Controller
    {
        private readonly IMovieSubTitleService _movieSubTitleService;
        public MovieSubTitleController(IMovieSubTitleService movieSubTitleService)
        {
            _movieSubTitleService = movieSubTitleService;
        }
        [HttpPost("UploadMovieSubTitle")]
        public async Task<IActionResult> UploadMovieSubTitle([FromForm] AutoGenerateSubTitleRequest autoGenerateSubTitleRequest, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.AutoGenerateSubTitleAsync(autoGenerateSubTitleRequest, ct);
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
        [HttpGet("GetAllSubTitlesBySourceID/{sourceID}")]
        public async Task<IActionResult> GetAllSubTitlesByMovieId(int sourceID, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.GetMovieSubTitlesByMovieSourceID(sourceID, ct);
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
