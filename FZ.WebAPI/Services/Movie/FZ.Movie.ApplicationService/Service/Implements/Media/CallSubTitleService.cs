using FZ.Constant;
using FZ.Movie.Domain.Media;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Dtos.Respone;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Media;
using FZ.Shared.ApplicationService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media
{
    public interface ITranscribeIntegrationService
    {
        Task<ResponseDto<string>> SendRequestAsync(AutoGenerateSubTitleRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> HandleCallbackAsync(TranscribeCallbackRequest request, CancellationToken ct);

        Task<ResponseDto<string>> TranslateFromRawDataAsync(TranslateSourceRawRequest request, CancellationToken ct);
    }

    public class TranscribeIntegrationService : ITranscribeIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TranscribeIntegrationService> _logger;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMovieSubTitleRepository _movieSubTitleRepository;
        private readonly IMovieSourceRepository _movieSourceRepository;
        private readonly IEpisodeSubTitleRepository _episodeSubTitleRepository;
        private readonly IEpisodeSourceRepository _episodeSourceRepository;
        // private readonly IEpisodeSourceRepository _episodeSourceRepository; // Inject thêm nếu cần update raw data cho episode

        public TranscribeIntegrationService(
            IHttpClientFactory httpClientFactory,
            ILogger<TranscribeIntegrationService> logger,
            ICloudinaryService cloudinaryService,
            IUnitOfWork unitOfWork,
            IMovieSubTitleRepository movieSubTitleRepository,
            IEpisodeSourceRepository episodeSourceRepository,
            IMovieSourceRepository movieSourceRepository,
            IEpisodeSubTitleRepository episodeSubTitleRepository)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
            _movieSubTitleRepository = movieSubTitleRepository;
            _movieSourceRepository = movieSourceRepository;
            _episodeSourceRepository = episodeSourceRepository;
            _episodeSubTitleRepository = episodeSubTitleRepository;
        }
        public async Task<ResponseDto<string>> TranslateFromRawDataAsync(TranslateSourceRawRequest request, CancellationToken ct)
        {
            try
            {
                string rawSubTitleJson = string.Empty;

                // 1. Tìm kiếm Raw Data từ DB dựa trên Type
                if (request.type.Equals("movie", StringComparison.OrdinalIgnoreCase))
                {
                    var movieSource = await _movieSourceRepository.GetByIdAsync(request.sourceID, ct);
                    if (movieSource == null)
                        return ResponseConst.Error<string>(404, "Movie Source không tồn tại.");

                    rawSubTitleJson = movieSource.rawSubTitle;
                }
                else if (request.type.Equals("episode", StringComparison.OrdinalIgnoreCase))
                {
                    var episodeSource = await _episodeSourceRepository.GetByIdAsync(request.sourceID, ct);
                    if (episodeSource == null)
                        return ResponseConst.Error<string>(404, "Episode Source không tồn tại.");

                    rawSubTitleJson = episodeSource.rawSubTitle;
                }
                else
                {
                    return ResponseConst.Error<string>(400, "Type không hợp lệ (chỉ chấp nhận 'movie' hoặc 'episode').");
                }

                // 2. Validate Raw Data
                if (string.IsNullOrWhiteSpace(rawSubTitleJson))
                {
                    return ResponseConst.Error<string>(400, "Source này chưa có dữ liệu Raw SubTitle.");
                }

                // 3. Deserialize chuỗi JSON từ DB thành List Object
                // DB lưu: "[{\"start\":0...}]" -> Cần chuyển thành List<RawSegmentDto>
                List<RawSegmentDto> segments;
                try
                {
                    segments = JsonSerializer.Deserialize<List<RawSegmentDto>>(rawSubTitleJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch
                {
                    return ResponseConst.Error<string>(500, "Lỗi format JSON trong Database.");
                }

                if (segments == null || !segments.Any())
                    return ResponseConst.Error<string>(400, "Raw SubTitle rỗng.");

                // 4. Chuẩn bị gọi External API
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                var targetUrl = request.externalApiUrl.TrimEnd('/') + "/translate/segments";

                if (!string.IsNullOrEmpty(request.apiToken))
                {
                    if (client.DefaultRequestHeaders.Contains("x-token"))
                        client.DefaultRequestHeaders.Remove("x-token");
                    client.DefaultRequestHeaders.Add("x-token", request.apiToken);
                }

                // 5. Đóng gói Payload gửi đi
                var payload = new
                {
                    segments = segments,           // List object đã deserialize
                    language = request.targetLanguage,
                    type = request.type.ToLower(),
                    source_id = request.sourceID.ToString()
                };

                // 6. Gửi Request
                var response = await client.PostAsJsonAsync(targetUrl, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync(ct);
                    return ResponseConst.Error<string>((int)response.StatusCode, $"AI Service Error: {msg}");
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                if (result.TryGetProperty("task_id", out var taskIdElement))
                {
                    return ResponseConst.Success("Đã gửi yêu cầu dịch thành công.", taskIdElement.GetString());
                }

                return ResponseConst.Success("Thành công (Không có TaskID).", "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TranslateFromRawData Error");
                return ResponseConst.Error<string>(500, ex.Message);
            }
        }

        public async Task<ResponseDto<string>> SendRequestAsync(AutoGenerateSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(30);

                var targetUrl = request.externalApiUrl.TrimEnd('/') + "/transcribe/process";

                using var content = new MultipartFormDataContent();
                using var fileStream = request.videoFile.OpenReadStream();

                content.Add(new StreamContent(fileStream), "file", request.videoFile.FileName);
                content.Add(new StringContent("turbo"), "model_id");
                content.Add(new StringContent("srt"), "output_format");

                // QUAN TRỌNG: Gửi Type và SourceID đi
                content.Add(new StringContent(request.type.ToLower()), "type");
                content.Add(new StringContent(request.sourceID.ToString()), "source_id");

                if (!string.IsNullOrEmpty(request.apiToken))
                {
                    client.DefaultRequestHeaders.Add("x-token", request.apiToken);
                }

                var response = await client.PostAsync(targetUrl, content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync(ct);
                    return ResponseConst.Error<string>((int)response.StatusCode, msg);
                }

                // Giả sử API trả về JSON: { "task_id": "..." }
                var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                string taskId = result.GetProperty("task_id").GetString();

                return ResponseConst.Success("Gửi yêu cầu thành công", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Integration Service Error");
                return ResponseConst.Error<string>(500, ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> HandleCallbackAsync(TranscribeCallbackRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Callback Received: Type={Type}, ID={Id}", request.type, request.sourceID);

            // 1. Upload SRT (Logic chung)
            var srtFileName = $"{request.type}_{request.sourceID}_{DateTime.UtcNow.Ticks}.srt";
            var srtBytes = Encoding.UTF8.GetBytes(request.srt);
            using var stream = new MemoryStream(srtBytes);
            var formFile = new FormFile(stream, 0, srtBytes.Length, "file", srtFileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var cloudUrl = await _cloudinaryService.UploadSrtAsync(formFile);
            if (string.IsNullOrEmpty(cloudUrl)) return ResponseConst.Error<bool>(500, "Upload Cloudinary Failed");

            // 2. Phân loại xử lý
            switch (request.type?.ToLower())
            {
                case "movie":
                    return await SaveMovieData(request, cloudUrl, ct);
                case "episode":
                    return await SaveEpisodeData(request, cloudUrl, ct);
                default:
                    return ResponseConst.Error<bool>(400, "Invalid Type");
            }
        }

        private async Task<ResponseDto<bool>> SaveMovieData(TranscribeCallbackRequest request, string cloudUrl, CancellationToken ct)
        {
            var source = await _movieSourceRepository.GetByIdAsync(request.sourceID, ct);
            if (source == null) return ResponseConst.Error<bool>(404, "Movie Source Not Found");

            var sub = new MovieSubTitle
            {
                movieSourceID = request.sourceID,
                subTitleName = "Auto Generated",
                language = request.language,
                linkSubTitle = cloudUrl,
                isActive = true,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            source.rawSubTitle = JsonSerializer.Serialize(request.RawSegments);
            source.updatedAt = DateTime.UtcNow;

            await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                await _movieSubTitleRepository.AddAsync(sub, token);
                await _movieSourceRepository.UpdateAsync(source);
                return true;
            }, ct: ct);

            return ResponseConst.Success("Saved Movie Subtitle", true);
        }

        private async Task<ResponseDto<bool>> SaveEpisodeData(TranscribeCallbackRequest request, string cloudUrl, CancellationToken ct)
        {

            var source = await _episodeSourceRepository.GetByIdAsync(request.sourceID, ct);
            if (source == null) return ResponseConst.Error<bool>(404, "Movie Source Not Found");

            var sub = new EpisodeSubTitle
            {
                episodeSourceID = request.sourceID,
                subTitleName = "Auto Generated",
                language = request.language,
                linkSubTitle = cloudUrl,
                isActive = true,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            // Có thể update rawSubTitle cho EpisodeSource tại đây nếu cần
            source.rawSubTitle = JsonSerializer.Serialize(request.RawSegments);
            source.updatedAt = DateTime.UtcNow;

            await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                await _episodeSubTitleRepository.AddAsync(sub, token);
                await _episodeSourceRepository.UpdateAsync(source);
                return true;
            }, ct: ct);

            return ResponseConst.Success("Saved Episode Subtitle", true);
        }
    }

}
