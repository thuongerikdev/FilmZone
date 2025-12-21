using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.ApplicationService.Service.Implements.Media;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Dtos.Respone;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MovieSubTitleController : Controller
    {
        private readonly IMovieSubTitleService _movieSubTitleService;
        private readonly IEpisodeSubTitleService _episodeSubTitleService;
        private readonly ITranscribeIntegrationService _transcribeService;
        public MovieSubTitleController(
            IMovieSubTitleService movieSubTitleService,
            ITranscribeIntegrationService transcribeIntegrationService,
            IEpisodeSubTitleService episodeSubTitleService)
        {
            _movieSubTitleService = movieSubTitleService;
            _transcribeService = transcribeIntegrationService;
            _episodeSubTitleService = episodeSubTitleService;
        }
        [HttpPost("UploadMovieSubTitle")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(
            MultipartBodyLengthLimit = long.MaxValue,
            ValueLengthLimit = int.MaxValue,
            MultipartHeadersLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadMovieSubTitle([FromForm] AutoGenerateSubTitleRequest autoGenerateSubTitleRequest, CancellationToken ct)
        {
            try
            {
                // Gọi service mới (trả về TaskID)
                var result = await _transcribeService.SendRequestAsync(autoGenerateSubTitleRequest, ct);

                if (result.ErrorCode != 200)
                {
                    return BadRequest(result);
                }

                // Trả về TaskID cho client biết là đã gửi thành công
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi yêu cầu tạo phụ đề.", details = ex.Message });
            }
        }

        // 2. API NHẬN CALLBACK (New)
        // Bên thứ 3 (AI Service) sẽ gọi API này khi xử lý xong
        // Lưu ý: Dùng [FromBody] vì Service thường gửi JSON payload, không phải Form-Data
        [HttpPost("Callback/TranscribeResult")]
        public async Task<IActionResult> ReceiveTranscribeCallback([FromBody] TranscribeCallbackRequest callbackRequest, CancellationToken ct)
        {
            try
            {
                // Gọi service xử lý dữ liệu trả về (Lưu DB, Upload Cloudinary)
                var result = await _transcribeService.HandleCallbackAsync(callbackRequest, ct);

                if (result.ErrorCode != 200)
                {
                    // Trả về lỗi để bên thứ 3 biết (nếu họ có cơ chế retry)
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xử lý callback.", details = ex.Message });
            }
        }
        [HttpGet("movie/GetAllSubTitlesBySourceID/{sourceID}")]
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
        [HttpGet("movie/GetAllSubTitles")]
        public async Task<IActionResult> GetAllSubTitles(CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.GetAllMovieSubTitile(ct);
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
        [HttpPost("movie/createMovieSubTitle")]
        public async Task<IActionResult> CreateMovieSubTitle([FromBody] CreateMovieSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.CreateMovieSubTitle(request, ct);
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
        [HttpPut("movie/updateMovieSubTitle")]
        public async Task<IActionResult> UpdateMovieSubTitle([FromBody] UpdateMovieSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.UpdateMovieSubTitle(request, ct);
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
        [HttpDelete("movie/deleteMovieSubTitle/{movieSubTitleID}")]
        public async Task<IActionResult> DeleteMovieSubTitle(int movieSubTitleID, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.DeleteMovieSubTitle(movieSubTitleID, ct);
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
        [HttpGet("movie/GetMovieSubTitleByID/{movieSubTitleID}")]
        public async Task<IActionResult> GetMovieSubTitleByID(int movieSubTitleID, CancellationToken ct)
        {
            try
            {
                var result = await _movieSubTitleService.GetMovieSubTitleByID(movieSubTitleID, ct);
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


        [HttpPost("episode/createEpisodeSubTitle")]
        public async Task<IActionResult> CreateEpisodeSubTitle([FromBody] CreateEpisodeSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.CreateEpisodeSubTitle(request, ct);
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
        [HttpPut("episode/updateEpisodeSubTitle")]
        public async Task<IActionResult> UpdateEpisodeSubTitle([FromBody] UpdateEpisodeSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.UpdateEpisodeSubTitle(request, ct);
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
        [HttpDelete("episode/deleteEpisodeSubTitle/{episodeSubTitleID}")]
        public async Task<IActionResult> DeleteEpisodeSubTitle(int episodeSubTitleID, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.DeleteEpisodeSubTitle(episodeSubTitleID, ct);
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
        [HttpGet("episode/GetAllSubTitlesBySourceID/{sourceID}")]
        public async Task<IActionResult> GetAllSubTitlesByEpisodeId(int sourceID, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.GetEpisodeSubTitlesByEpisodeSourceID(sourceID, ct);
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
        [HttpGet("episode/GetEpisodeSubTitleByID/{episodeSubTitleID}")]
        public async Task<IActionResult> GetEpisodeSubTitleByID(int episodeSubTitleID, CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.GetEpisodeSubTitleByID(episodeSubTitleID, ct);
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
      
        [HttpGet("episode/GetAllSubTitles")]
        public async Task<IActionResult> GetAllEpisodeSubTitles(CancellationToken ct)
        {
            try
            {
                var result = await _episodeSubTitleService.GetAllEpisodeSubTitile(ct);
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
