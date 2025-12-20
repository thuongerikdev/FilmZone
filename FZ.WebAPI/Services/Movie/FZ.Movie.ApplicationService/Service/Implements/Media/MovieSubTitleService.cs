using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
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
    public class MovieSubTitleService : MovieServiceBase, IMovieSubTitleService
    {
        private readonly IMovieSubTitleRepository _movieSubTitleRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMovieSourceRepository _movieSourceRepository;
        private IUnitOfWork _unitOfWork;
        public MovieSubTitleService(
            IMovieSubTitleRepository movieSubTitleRepository,
            IUnitOfWork unitOfWork,
            ILogger<MovieSubTitleService> logger,
            IHttpClientFactory httpClientFactory,
            IMovieSourceRepository movieSourceRepository,
            ICloudinaryService cloudinaryService) : base(logger)
        {
            _movieSubTitleRepository = movieSubTitleRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _httpClientFactory = httpClientFactory;
            _movieSourceRepository = movieSourceRepository;

        }






        public async Task<ResponseDto<MovieSubTitle>> CreateMovieSubTitle(CreateMovieSubTitleRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating MovieSubTitle");
            try
            {
                var movieSubTitle = new MovieSubTitle
                {
                    movieSourceID = request.movieSourceID,
                    language = request.language,
                    linkSubTitle = request.linkSubTitle,
                    isActive = request.isActive,
                    subTitleName = request.subTitleName,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _movieSubTitleRepository.AddAsync(movieSubTitle, token);
                    return movieSubTitle;

                }, ct: ct);
                _logger.LogInformation("MovieSubTitle created successfully with ID: {MovieSubTitleID}", movieSubTitle.movieSubTitleID);
                return ResponseConst.Success("Tạo thành công", movieSubTitle);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating MovieSubTitle");
                return ResponseConst.Error<MovieSubTitle>(500, "Có lỗi xảy ra");
            }

        }
        public async Task<ResponseDto<MovieSubTitle>> UpdateMovieSubTitle(UpdateMovieSubTitleRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating MovieSubTitle with ID: {MovieSubTitleID}", request.movieSubTitleID);
            try
            {
                var existingMovieSubTitle = await _movieSubTitleRepository.GetTrackedAsync(request.movieSubTitleID, ct);
                if (existingMovieSubTitle == null)
                {
                    _logger.LogWarning("MovieSubTitle with ID: {MovieSubTitleID} not found", request.movieSubTitleID);
                    return ResponseConst.Error<MovieSubTitle>(404, "Không tìm thấy phụ đề");
                }
                existingMovieSubTitle.movieSourceID = request.movieSourceID;
                existingMovieSubTitle.language = request.language;
                existingMovieSubTitle.linkSubTitle = request.linkSubTitle;
                existingMovieSubTitle.isActive = request.isActive;
                existingMovieSubTitle.subTitleName = request.subTitleName;
                existingMovieSubTitle.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _movieSubTitleRepository.UpdateAsync(existingMovieSubTitle);
                    return existingMovieSubTitle;
                }, ct: ct);
                _logger.LogInformation("MovieSubTitle with ID: {MovieSubTitleID} updated successfully", request.movieSubTitleID);
                return ResponseConst.Success("Cập nhật thành công", existingMovieSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating MovieSubTitle with ID: {MovieSubTitleID}", request.movieSubTitleID);
                return ResponseConst.Error<MovieSubTitle>(500, "Có lỗi xảy ra");
            }
        }

        public async Task<ResponseDto<bool>> DeleteMovieSubTitle(int movieSubTitleID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting MovieSubTitle with ID: {MovieSubTitleID}", movieSubTitleID);
            try
            {
                var existingMovieSubTitle = await _movieSubTitleRepository.GetByIdAsync(movieSubTitleID, ct);
                if (existingMovieSubTitle == null)
                {
                    _logger.LogWarning("MovieSubTitle with ID: {MovieSubTitleID} not found", movieSubTitleID);
                    return ResponseConst.Error<bool>(404, "Không tìm thấy phụ đề");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _movieSubTitleRepository.RemoveAsync(movieSubTitleID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("MovieSubTitle with ID: {MovieSubTitleID} deleted successfully", movieSubTitleID);
                return ResponseConst.Success("Xóa thành công", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting MovieSubTitle with ID: {MovieSubTitleID}", movieSubTitleID);
                return ResponseConst.Error<bool>(500, "Có lỗi xảy ra");
            }
        }
        public async Task<ResponseDto<MovieSubTitle>> GetMovieSubTitleByID(int movieSubTitleID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving MovieSubTitle with ID: {MovieSubTitleID}", movieSubTitleID);
            try
            {
                var movieSubTitle = await _movieSubTitleRepository.GetByIdAsync(movieSubTitleID, ct);
                if (movieSubTitle == null)
                {
                    _logger.LogWarning("MovieSubTitle with ID: {MovieSubTitleID} not found", movieSubTitleID);
                    return ResponseConst.Error<MovieSubTitle>(404, "Không tìm thấy phụ đề");
                }
                _logger.LogInformation("MovieSubTitle with ID: {MovieSubTitleID} retrieved successfully", movieSubTitleID);
                return ResponseConst.Success("Lấy dữ liệu thành công", movieSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving MovieSubTitle with ID: {MovieSubTitleID}", movieSubTitleID);
                return ResponseConst.Error<MovieSubTitle>(500, "Có lỗi xảy ra");
            }
        }

        public async Task<ResponseDto<List<MovieSubTitle>>> GetMovieSubTitlesByMovieSourceID(int movieSourceID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving MovieSubTitles for MovieSourceID: {MovieSourceID}", movieSourceID);
            try
            {
                var movieSubTitles = await _movieSubTitleRepository.GetByMovieSourceIDAsync(movieSourceID, ct);
                _logger.LogInformation("MovieSubTitles for MovieSourceID: {MovieSourceID} retrieved successfully", movieSourceID);
                return ResponseConst.Success("Lấy dữ liệu thành công", movieSubTitles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving MovieSubTitles for MovieSourceID: {MovieSourceID}", movieSourceID);
                return ResponseConst.Error<List<MovieSubTitle>>(500, "Có lỗi xảy ra");
            }
        }
        public async Task<ResponseDto<List<MovieSubTitle>>> GetAllMovieSubTitile(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all MovieSubTitles");
            try
            {
                var movieSubTitles = await _movieSubTitleRepository.GetAllMovieSubTitleAsync(ct);
                _logger.LogInformation("All MovieSubTitles retrieved successfully");
                return ResponseConst.Success("Lấy dữ liệu thành công", movieSubTitles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all MovieSubTitles");
                return ResponseConst.Error<List<MovieSubTitle>>(500, "Có lỗi xảy ra");
            }
        }
        //AutoGenerateSubTitleAsync 

        public async Task<ResponseDto<string>> AutoGenerateSubTitleAsync(AutoGenerateSubTitleRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Start sending video to AI Service. SourceID: {SourceID}, URL: {Url}", request.movieSourceID, request.externalApiUrl);

            // 1. Validate Input
            if (request.videoFile == null || request.videoFile.Length == 0)
                return ResponseConst.Error<string>(400, "File video không hợp lệ");

            if (string.IsNullOrEmpty(request.externalApiUrl))
                return ResponseConst.Error<string>(400, "URL API không được để trống");

            // 2. Kiểm tra MovieSource có tồn tại không
            var existingSource = await _movieSourceRepository.GetByIdAsync(request.movieSourceID, ct);
            if (existingSource == null)
            {
                return ResponseConst.Error<string>(404, "Không tìm thấy Movie Source");
            }

            try
            {
                // 3. Gọi API Python để lấy Task ID
                var taskResponse = await CallTranscribeApiAsync(request, ct);

                if (taskResponse == null || string.IsNullOrEmpty(taskResponse.task_id))
                {
                    return ResponseConst.Error<string>(500, "API AI không trả về Task ID hoặc bị lỗi");
                }

                _logger.LogInformation("Task started successfully. TaskID: {TaskId}", taskResponse.task_id);

                // Lưu ý: Ở đây bạn có thể lưu TaskID vào DB nếu cần theo dõi tiến độ, 
                // nhưng yêu cầu hiện tại chỉ cần trả về kết quả.

                return ResponseConst.Success("Đã gửi yêu cầu xử lý thành công. Vui lòng chờ Callback.", taskResponse.task_id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI Service for SourceID: {SourceID}", request.movieSourceID);
                return ResponseConst.Error<string>(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        /// <summary>
        /// Bước 2: Hàm Webhook để bên thứ 3 gọi lại khi có kết quả (SRT, Language, Segments)
        /// </summary>
        public async Task<ResponseDto<MovieSubTitle>> ProcessTranscribeCallbackAsync(TranscribeCallbackRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Received Callback for MovieSourceID: {SourceID}", request.movieSourceID);

            // 1. Validate logic
            if (string.IsNullOrEmpty(request.srt))
            {
                return ResponseConst.Error<MovieSubTitle>(400, "Dữ liệu SRT rỗng");
            }

            var existingSource = await _movieSourceRepository.GetByIdAsync(request.movieSourceID, ct);
            if (existingSource == null)
            {
                return ResponseConst.Error<MovieSubTitle>(404, $"Không tìm thấy Movie Source với ID {request.movieSourceID}");
            }

            try
            {
                // 2. Upload file SRT lên Cloudinary
                // Tạo tên file ảo
                var srtFileName = $"sub_{request.movieSourceID}_{DateTime.UtcNow.Ticks}.srt";
                var srtFile = ConvertStringToFormFile(request.srt, srtFileName);

                var cloudUrl = await _cloudinaryService.UploadSrtAsync(srtFile);

                if (string.IsNullOrEmpty(cloudUrl))
                {
                    return ResponseConst.Error<MovieSubTitle>(500, "Lỗi khi upload file phụ đề lên Cloudinary");
                }

                // 3. Prepare Data

                // A. Tạo SubTitle mới
                var movieSubTitle = new MovieSubTitle
                {
                    movieSourceID = request.movieSourceID,
                    subTitleName = $"Auto Generated ({request.language ?? "unk"})",
                    linkSubTitle = cloudUrl,
                    language = request.language ?? "unknown",
                    isActive = true,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                // B. Cập nhật Raw Segments vào MovieSource
                if (request.RawSegments != null && request.RawSegments.Any())
                {
                    existingSource.rawSubTitle = JsonSerializer.Serialize(request.RawSegments);
                }
                else
                {
                    existingSource.rawSubTitle = "[]";
                }
                existingSource.updatedAt = DateTime.UtcNow;

                // 4. Save to DB (Transaction)
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _movieSubTitleRepository.AddAsync(movieSubTitle, token);
                    await _movieSourceRepository.UpdateAsync(existingSource);
                    return movieSubTitle;
                }, ct: ct);

                _logger.LogInformation("Callback processed successfully. New SubID: {SubId}", movieSubTitle.movieSubTitleID);
                return ResponseConst.Success("Xử lý callback và tạo phụ đề thành công", movieSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing callback for SourceID: {SourceID}", request.movieSourceID);
                return ResponseConst.Error<MovieSubTitle>(500, "Lỗi xử lý callback: " + ex.Message);
            }
        }


        // --- PRIVATE HELPER METHODS ---

        private async Task<TranscribeTaskResponse?> CallTranscribeApiAsync(AutoGenerateSubTitleRequest request, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            // --- XỬ LÝ URL ---
            // 1. Lấy URL từ request
            var targetUrl = request.externalApiUrl;

            // 2. Logic kiểm tra: Nếu URL chưa có đuôi "/transcribe/process" thì tự động nối vào
            // Cách này giúp user nhập "http://localhost:8000" hay "http://localhost:8000/" đều chạy đúng
            if (!targetUrl.EndsWith("/transcribe/process"))
            {
                // TrimEnd('/') để xóa dấu gạch chéo thừa ở cuối nếu user lỡ nhập (vd: ...8000/)
                targetUrl = targetUrl.TrimEnd('/') + "/transcribe/process";
            }

            // Setup Multipart Form Data
            using var content = new MultipartFormDataContent();
            using var fileStream = request.videoFile.OpenReadStream();

            // 1. Add File
            content.Add(new StreamContent(fileStream), "file", request.videoFile.FileName);

            // 2. Add Parameters
            content.Add(new StringContent("turbo"), "model_id");
            content.Add(new StringContent("srt"), "output_format");

            // 3. Add Headers
            if (!string.IsNullOrEmpty(request.apiToken))
            {
                client.DefaultRequestHeaders.Add("x-token", request.apiToken);
            }
            client.DefaultRequestHeaders.Add("accept", "application/json");

            // 4. Call API (Dùng targetUrl đã xử lý thay vì request.externalApiUrl gốc)
            var response = await client.PostAsync(targetUrl, content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Transcribe API Error: {Code} - {Msg}", response.StatusCode, errorMsg);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TranscribeTaskResponse>(cancellationToken: ct);
        }

        private IFormFile ConvertStringToFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }
    }







}

