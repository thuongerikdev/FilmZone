using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class RegionController : Controller
    {
       private readonly IRegionService _regionService;
         public RegionController(IRegionService regionService)
         {
              _regionService = regionService;
        }
        [HttpPost]
        [Authorize(Policy = "RegionManage")]
        public async Task<IActionResult> CreateRegion( CreateRegionRequest createRegionRequest, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(createRegionRequest.name))
            {
                return BadRequest("Region name cannot be empty.");
            }
            try
            {
                var result = await _regionService.CreateRegion(createRegionRequest, ct);
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
        [Authorize(Policy = "RegionManage")]
        public async Task<IActionResult> UpdateRegion(UpdateRegionRequest updateRegionRequest, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(updateRegionRequest.name))
            {
                return BadRequest("Region name cannot be empty.");
            }
            try
            {
                var result = await _regionService.UpdateRegion(updateRegionRequest, ct);
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
        [Authorize(Policy = "RegionManage")]
        public async Task<IActionResult> DeleteRegion(int id, CancellationToken ct)
        {
            try
            {
                var result = await _regionService.DeleteRegion(id, ct);
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
        [HttpGet("{ID}")]
        [Authorize(Policy = "RegionRead")]
        public async Task<IActionResult> GetRegionByID(int ID, CancellationToken ct)
        {
            try
            {
                var result = await _regionService.GetRegionByID(ID, ct);
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
        [HttpGet("getAll")]
        [Authorize(Policy = "RegionRead")]
        public async Task<IActionResult> GetAllRegions(CancellationToken ct)
        {
            try
            {
                var result = await _regionService.GetAllRegions( ct);
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
        [HttpGet("getMovieByRegionID/{regionID}")]
        [Authorize(Policy = "RegionRead")]
        public async Task<IActionResult> GetMovieByRegionID(int regionID, CancellationToken ct)
        {
            try
            {
                var result = await _regionService.GetMoviesByRegionIDAsync(regionID, ct);
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
        [HttpGet("getPersonByRegionID/{regionID}")]
        [Authorize(Policy = "RegionRead")]
        public async Task<IActionResult> GetPersonByRegionID(int regionID, CancellationToken ct)
        {
            try
            {
                var result = await _regionService.GetPeopleByRegionID(regionID, ct);
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
