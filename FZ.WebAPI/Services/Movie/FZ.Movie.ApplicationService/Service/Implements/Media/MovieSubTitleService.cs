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

        public async Task<ResponseDto<MovieSubTitle>> AutoGenerateSubTitleAsync(AutoGenerateSubTitleRequest autoGenerateSubTitleRequest, CancellationToken ct)
        {
            _logger.LogInformation("Start auto-generating subtitle for MovieSourceID: {SourceID}", autoGenerateSubTitleRequest.movieSourceID);

            // 1. Validate Input
            if (autoGenerateSubTitleRequest.videoFile == null || autoGenerateSubTitleRequest.videoFile.Length == 0)
                return ResponseConst.Error<MovieSubTitle>(400, "File video không hợp lệ");

            // 2. Kiểm tra MovieSource có tồn tại không
            // Cần lấy entity ra để tí nữa còn update cột rawSubTitle
            var existingSource = await _movieSourceRepository.GetByIdAsync(autoGenerateSubTitleRequest.movieSourceID, ct);
            if (existingSource == null)
            {
                return ResponseConst.Error<MovieSubTitle>(404, "Không tìm thấy Movie Source");
            }

            try
            {
                // 3. Gọi API Python Local để Transcribe
                // Hàm này sẽ trả về SRT và List Segments
                var transcribeData = await CallTranscribeApiAsync(autoGenerateSubTitleRequest.videoFile, ct);

                if (transcribeData == null || string.IsNullOrEmpty(transcribeData.srt))
                {
                    return ResponseConst.Error<MovieSubTitle>(500, "API AI trả về dữ liệu rỗng");
                }

                // 4. Upload file SRT lên Cloudinary
                // Tạo file ảo từ chuỗi SRT
                var srtFileName = Path.GetFileNameWithoutExtension(autoGenerateSubTitleRequest.videoFile.FileName) + ".srt";
                var srtFile = ConvertStringToFormFile(transcribeData.srt, srtFileName);

                // Upload
                var cloudUrl = await _cloudinaryService.UploadSrtAsync(srtFile);

                if (string.IsNullOrEmpty(cloudUrl))
                {
                    return ResponseConst.Error<MovieSubTitle>(500, "Lỗi khi upload file phụ đề lên Cloudinary");
                }

                // 5. Cập nhật dữ liệu vào Object

                // A. Tạo SubTitle mới (Lưu link file srt)
                var movieSubTitle = new MovieSubTitle
                {
                    movieSourceID = autoGenerateSubTitleRequest.movieSourceID,
                    subTitleName = srtFileName + " (Auto Generated)",
                    linkSubTitle = cloudUrl,
                    language = transcribeData.language ?? "unknown",
                    isActive = true,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                // B. Cập nhật MovieSource (Lưu JSON raw_segments)
                if (transcribeData.RawSegments != null && transcribeData.RawSegments.Any())
                {
                    // Serialize list object thành chuỗi JSON để lưu vào DB
                    // Cột rawSubTitle trong DB phải là kiểu nvarchar(max) hoặc text
                    existingSource.rawSubTitle = JsonSerializer.Serialize(transcribeData.RawSegments);
                }
                else
                {
                    // Nếu không có segment thì lưu tạm mảng rỗng hoặc giữ nguyên
                    existingSource.rawSubTitle = "[]";
                }
                existingSource.updatedAt = DateTime.UtcNow;

                // 6. Thực thi Transaction (Lưu cả 2 bảng cùng lúc)
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    // Lưu bảng SubTitle
                    await _movieSubTitleRepository.AddAsync(movieSubTitle, token);

                    // Update bảng Source
                    await _movieSourceRepository.UpdateAsync(existingSource); // Đảm bảo Repo có hàm UpdateAsync hỗ trợ UnitOfWork

                    return movieSubTitle;
                }, ct: ct);

                _logger.LogInformation("Auto-generated subtitle success. ID: {SubId}", movieSubTitle.movieSubTitleID);
                return ResponseConst.Success("Tạo phụ đề thành công", movieSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-generating subtitle for SourceID: {SourceID}", autoGenerateSubTitleRequest.movieSourceID);
                return ResponseConst.Error<MovieSubTitle>(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // --- PRIVATE HELPER METHODS ---

        private async Task<TranscribeApiResponse?> CallTranscribeApiAsync(IFormFile videoFile, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();

            // Tăng timeout vì upload video và chờ AI xử lý rất lâu
            client.Timeout = TimeSpan.FromMinutes(10);

            // URL API Python
            var apiUrl = "http://localhost:8080/transcribe/audio_video_2_srt";

            using var content = new MultipartFormDataContent();
            using var fileStream = videoFile.OpenReadStream();

            // Tham số "file" phải khớp với bên Python (FastAPI: file: UploadFile)
            content.Add(new StreamContent(fileStream), "file", videoFile.FileName);

            var response = await client.PostAsync(apiUrl, content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Transcribe API Error: {Code} - {Msg}", response.StatusCode, errorMsg);
                return null;
            }

            // Map JSON response về DTO
            return await response.Content.ReadFromJsonAsync<TranscribeApiResponse>(cancellationToken: ct);
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

