using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class EpisodeSourceController : Controller
    {
        private readonly IEpisodeSourceService _episodeSourceService;
        public EpisodeSourceController(IEpisodeSourceService episodeSourceService)
        {
            _episodeSourceService = episodeSourceService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateEpisodeSource( [FromBody] CreateEpisodeSourceRequest createEpisodeSourceRequest,   CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeSourceService.CreateEpisodeSource(createEpisodeSourceRequest ,ct);
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
        public async Task<IActionResult> UpdateEpisodeSource( [FromBody] UpdateEpisodeSourceRequest updateEpisodeSourceRequest,   CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _episodeSourceService.UpdateEpisodeSource(updateEpisodeSourceRequest ,ct);
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
        public async Task<IActionResult> DeleteEpisodeSource(int id, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSourceService.DeleteEpisodeSource(id, ct);
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
        public async Task<IActionResult> GetEpisodeSourceById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSourceService.GetEpisodeSourceByID(id, ct);
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
        [HttpGet("{episodeId}")] 
        public async Task<IActionResult> GetEpisodeSourcesByEpisodeId(int episodeId, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSourceService.GetEpisodeSourcesByEpisodeID(episodeId, ct);
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
