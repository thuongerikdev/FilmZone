using FZ.Constant;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EpisodeController : Controller
    {
        private readonly IEpisodeService _episodeService;
        public EpisodeController(IEpisodeService episodeService)
        {
            _episodeService = episodeService;
        }
        [HttpPost]
        [Authorize(Policy = "EpisodeManage")]
        public async Task<IActionResult> CreateEpisode([FromBody] CreateEpisodeRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeService.CreateEpisode(request, ct);
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
        [Authorize(Policy = "EpisodeManage")]
        public async Task<IActionResult> UpdateEpisode([FromBody] UpdateEpisodeRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeService.UpdateEpisode(request, ct);
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
        [Authorize(Policy = "EpisodeManage")]
        public async Task<IActionResult> DeleteEpisode(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid episode ID.");
            }
            try
            {
                var result = await _episodeService.DeleteEpisode(id, ct);
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
        [Authorize(Policy = "EpisodeRead")]
        public async Task<IActionResult> GetEpisodeById(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid episode ID.");
            }
            try
            {
                var result = await _episodeService.GetEpisodeByID(id, ct);
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
        [HttpGet("getAll")]
        [Authorize(Policy = "EpisodeRead")]
        public async Task<IActionResult> GetAllEpisodes(CancellationToken ct)
        {
            try
            {
                var result = await _episodeService.GetAllEpisode(ct);
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
        [HttpGet("getbyMovie/{movieId}")]
        [Authorize(Policy = "EpisodeRead")]
        public async Task<IActionResult> GetEpisodesByMovieId(int movieId, CancellationToken ct)
        {
            if (movieId <= 0)
            {
                return BadRequest("Invalid movie ID.");
            }
            try
            {
                var result = await _episodeService.GetEpisodeByMovieID(movieId, ct);
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
