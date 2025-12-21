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
        private readonly ITranscribeIntegrationService _transcribeService;
        private IUnitOfWork _unitOfWork;
        public MovieSubTitleService(
            IMovieSubTitleRepository movieSubTitleRepository,
            IUnitOfWork unitOfWork,
            ILogger<MovieSubTitleService> logger,
            IHttpClientFactory httpClientFactory,
            IMovieSourceRepository movieSourceRepository,
            ITranscribeIntegrationService transcribeService,
            ICloudinaryService cloudinaryService) : base(logger)
        {
            _movieSubTitleRepository = movieSubTitleRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _httpClientFactory = httpClientFactory;
            _movieSourceRepository = movieSourceRepository;
            _transcribeService = transcribeService;

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

        //public async Task<ResponseDto<string>> AutoGenerateSubTitleAsync(AutoGenerateSubTitleRequest request, CancellationToken ct)
        //{
        //    // Set cứng type là MOVIE và sourceID tương ứng
        //    request.type = "movie";
        //    request.sourceID = request.sourceID; // Map từ movieSourceID sang sourceID chung

        //    return await _transcribeService.SendRequestAsync(request, ct);
        //}
    }







}

