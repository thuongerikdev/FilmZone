using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class ImageSourceController : Controller
    {
        private readonly IImageSourceService _imageSourceService;
        public ImageSourceController(IImageSourceService imageSourceService)
        {
            _imageSourceService = imageSourceService;
        }
        [HttpPost]
        [Authorize(Policy = "ImageManage")]
        public async Task<IActionResult> CreateImageSource([FromForm] CreateImageSourceRequest createImageSourceRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _imageSourceService.CreateImageSource(createImageSourceRequest, ct);
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
        [Authorize(Policy = "ImageManage")]
        public async Task<IActionResult> UpdateImageSource([FromForm] UpdateImageSourceRequest updateImageSourceRequest, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _imageSourceService.UpdateImageSource(updateImageSourceRequest, ct);
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
        [Authorize(Policy = "ImageManage")]
        public async Task<IActionResult> DeleteImageSource(int id, CancellationToken ct)
        {
            try
            {
                var result = await _imageSourceService.DeleteImageSource(id, ct);
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
        [HttpGet("{Type}")]
        [Authorize(Policy = "ImageRead")]
        public async Task<IActionResult> GetImageSourcesByType(string Type, CancellationToken ct)
        {
            try
            {
                var result = await _imageSourceService.GetImageSourcesByTpe(Type, ct);
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
