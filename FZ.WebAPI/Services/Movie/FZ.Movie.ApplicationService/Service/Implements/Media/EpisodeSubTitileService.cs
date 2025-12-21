using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Media;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Media;
using FZ.Shared.ApplicationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media
{
    public class EpisodeSubTitileService : MovieServiceBase ,IEpisodeSubTitleService
    {

        private IEpisodeSubTitleRepository _episodeSubTitleRepository;
        private IUnitOfWork _unitOfWork;
        private ICloudinaryService _cloudinaryService;


        public EpisodeSubTitileService(
            ILogger<EpisodeSubTitileService> logger,
            IEpisodeSubTitleRepository episodeSubTitleRepository,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService
            ) : base(logger)
        {
            _episodeSubTitleRepository = episodeSubTitleRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<ResponseDto<EpisodeSubTitle>> CreateEpisodeSubTitle(CreateEpisodeSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Tạo mới đối tượng EpisodeSubTitle từ request
                var episodeSubTitle = new EpisodeSubTitle
                {
                    episodeSourceID = request.episodeSourceID,
                    language = request.language,
                    subTitleName = request.subTitleName,
                    linkSubTitle = request.linkSubTitle,
                    isActive = request.isActive,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };
                // 2. Thêm vào repository
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _episodeSubTitleRepository.AddAsync(episodeSubTitle, token);
                    return episodeSubTitle;

                }, ct: ct);
                // 4. Trả về kết quả thành công
                return ResponseConst.Success("Episode subtitle created successfully.", episodeSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating episode subtitle.");
                return ResponseConst.Error<EpisodeSubTitle>(500, "Failed to create episode subtitle.");
            }
        }
        public async Task<ResponseDto<EpisodeSubTitle>> UpdateEpisodeSubTitle(UpdateEpisodeSubTitleRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Lấy đối tượng EpisodeSubTitle hiện có từ repository
                var existingSubTitle = await _episodeSubTitleRepository.GetTrackedAsync(request.episodeSubTitleID, ct);
                if (existingSubTitle == null)
                {
                    return ResponseConst.Error<EpisodeSubTitle>(404, "Episode subtitle not found.");
                }
                // 2. Cập nhật các thuộc tính từ request
                existingSubTitle.episodeSourceID = request.episodeSourceID;
                existingSubTitle.language = request.language;
                existingSubTitle.subTitleName = request.subTitleName;
                existingSubTitle.linkSubTitle = request.linkSubTitle;
                existingSubTitle.isActive = request.isActive;
                existingSubTitle.updatedAt = DateTime.UtcNow;
                // 3. Lưu thay đổi vào repository trong một transaction
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _episodeSubTitleRepository.UpdateAsync(existingSubTitle);
                    return existingSubTitle;
                }, ct: ct);
                // 4. Trả về kết quả thành công
                return ResponseConst.Success("Episode subtitle updated successfully.", existingSubTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating episode subtitle.");
                return ResponseConst.Error<EpisodeSubTitle>(500, "Failed to update episode subtitle.");
            }


        }
        public async Task<ResponseDto<bool>> DeleteEpisodeSubTitle(int episodeSubTitleID, CancellationToken ct)
        {
            try
            {
                // 1. Kiểm tra sự tồn tại của EpisodeSubTitle
                var existingSubTitle = await _episodeSubTitleRepository.GetByIdAsync(episodeSubTitleID, ct);
                if (existingSubTitle == null)
                {
                    return ResponseConst.Error<bool>(404, "Episode subtitle not found.");
                }
                // 2. Xoá EpisodeSubTitle trong một transaction
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    await _episodeSubTitleRepository.RemoveAsync(episodeSubTitleID);
                    return true;
                }, ct: ct);
                // 3. Trả về kết quả thành công
                return ResponseConst.Success("Episode subtitle deleted successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting episode subtitle.");
                return ResponseConst.Error<bool>(500, "Failed to delete episode subtitle.");
            }
        }
        public async Task<ResponseDto<List<EpisodeSubTitle>>> GetEpisodeSubTitlesByEpisodeSourceID(int episodeSourceID, CancellationToken ct)
        {
            try
            {
                var subTitles = await _episodeSubTitleRepository.GetByEpisodeSourceIDAsync(episodeSourceID, ct);
                return ResponseConst.Success("Episode subtitles retrieved successfully.", subTitles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving episode subtitles.");
                return ResponseConst.Error<List<EpisodeSubTitle>>(500, "Failed to retrieve episode subtitles.");
            }
        }
        public async Task<ResponseDto<List<EpisodeSubTitle>>> GetAllEpisodeSubTitile(CancellationToken ct)
        {
            try
            {
                var subTitles = await _episodeSubTitleRepository.GetAllEpisodeSubTitleAsync(ct);
                return ResponseConst.Success("Episode subtitles retrieved successfully.", subTitles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all episode subtitles.");
                return ResponseConst.Error<List<EpisodeSubTitle>>(500, "Failed to retrieve episode subtitles.");
            }
        }
        public async Task<ResponseDto<EpisodeSubTitle>> GetEpisodeSubTitleByID(int episodeSubTitleID, CancellationToken ct)
        {
            try
            {
                var subTitle = await _episodeSubTitleRepository.GetByIdAsync(episodeSubTitleID, ct);
                if (subTitle == null)
                {
                    return ResponseConst.Error<EpisodeSubTitle>(404, "Episode subtitle not found.");
                }
                return ResponseConst.Success("Episode subtitle retrieved successfully.", subTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving episode subtitle by ID.");
                return ResponseConst.Error<EpisodeSubTitle>(500, "Failed to retrieve episode subtitle.");
            }
        }
    }
}
