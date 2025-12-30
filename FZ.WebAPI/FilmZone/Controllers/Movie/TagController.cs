using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }
        [HttpPost]
        [Authorize(Policy = "TagManage")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest createTagRequest, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(createTagRequest.tagName))
            {
                return BadRequest("Tag name cannot be empty.");
            }
            try
            {
                var result = await _tagService.CreateTag(createTagRequest, ct);
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
        [Authorize(Policy = "TagManage")]
        public async Task<IActionResult> UpdateTag([FromBody] UpdateTagRequest updateTagRequest, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(updateTagRequest.tagName))
            {
                return BadRequest("Tag name cannot be empty.");
            }
            try
            {
                var result = await _tagService.UpdateTag(updateTagRequest, ct);
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
        [Authorize(Policy = "TagManage")]
        public async Task<IActionResult> DeleteTag(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid tag ID.");
            }
            try
            {
                var result = await _tagService.DeleteTag(id, ct);
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
        [HttpGet("{TagID}")]
        [Authorize(Policy = "TagRead")]
        public async Task<IActionResult> GetTagById(int TagID, CancellationToken ct)
        {
            if (TagID <= 0)
            {
                return BadRequest("Invalid tag ID.");
            }
            try
            {
                var result = await _tagService.GetTagByID(TagID, ct);
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
        [HttpGet("getALlTags")]
        [Authorize(Policy = "TagRead")]
        public async Task<IActionResult> GetAllTags(CancellationToken ct)
        {
            try
            {
                var result = await _tagService.GetAllTags(ct);
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
