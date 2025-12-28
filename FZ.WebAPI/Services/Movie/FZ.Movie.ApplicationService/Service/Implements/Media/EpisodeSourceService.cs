using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Media;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Media;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Media
{
    public class EpisodeSourceService : MovieServiceBase , IEpisodeSourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEpisodeSourceRepository _episodeSourceRepository;
        public EpisodeSourceService(ILogger<MovieServiceBase> logger, IUnitOfWork unitOfWork, IEpisodeSourceRepository episodeSourceRepository) : base(logger)
        {
            _unitOfWork = unitOfWork;
            _episodeSourceRepository = episodeSourceRepository;
        }
        public async Task<ResponseDto<EpisodeSource>> CreateEpisodeSource(CreateEpisodeSourceRequest request, CancellationToken ct)
        {
            _logger.LogInformation("CreateEpisodeSource ep:{EpisodeID} type:{Type} quality:{Quality}",
                request.episodeID, request.sourceType, request.quality);

            try
            {
                if (string.IsNullOrWhiteSpace(request.sourceUrl))
                    return ResponseConst.Error<EpisodeSource>(400, "sourceUrl is required");
                if (string.IsNullOrWhiteSpace(request.sourceName))
                    return ResponseConst.Error<EpisodeSource>(400, "sourceName is required");

                var normalizedType = request.sourceType?.Trim().ToLowerInvariant() ?? "custom";
                var normalizedId = request.sourceId?.Trim();

                // Nếu có sourceId (Vimeo/YouTube/Archive) thì ưu tiên kiểm tra trùng theo sourceId
                EpisodeSource? existing = null;
                if (!string.IsNullOrEmpty(normalizedId))
                {
                    existing = await _episodeSourceRepository.GetByCompositeKeyAsync(
                        request.episodeID, normalizedType, normalizedId, request.language, request.quality, ct);
                }

                if (existing != null)
                {
                    existing.sourceName = request.sourceName;
                    existing.sourceUrl = request.sourceUrl;
                    existing.isVipOnly = request.isVipOnly;
                    existing.isActive = request.isActive;
                    existing.updatedAt = DateTime.UtcNow;

                    await _unitOfWork.ExecuteInTransactionAsync(async token =>
                    {
                        await _episodeSourceRepository.UpdateAsync(existing, token);
                        return true;
                    }, ct: ct);

                    _logger.LogInformation("EpisodeSource upserted (updated) id:{Id}", existing.episodeSourceID);
                    return ResponseConst.Success("Episode source updated", existing);
                }
                else
                {
                    var newSource = new EpisodeSource
                    {
                        episodeID = request.episodeID,
                        sourceName = request.sourceName,
                        sourceType = normalizedType,
                        sourceUrl = request.sourceUrl,
                        sourceID = normalizedId,
                        quality = request.quality,
                        language = request.language,
                        isVipOnly = request.isVipOnly,
                        isActive = request.isActive,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        rawSubTitle = ""
                    };

                    await _unitOfWork.ExecuteInTransactionAsync(async token =>
                    {
                        await _episodeSourceRepository.Add(newSource, token);
                        return newSource;
                    }, ct: ct);

                    _logger.LogInformation("EpisodeSource created id:{Id}", newSource.episodeSourceID);
                    return ResponseConst.Success("Episode source created successfully", newSource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateEpisodeSource error ep:{EpisodeID}", request.episodeID);
                return ResponseConst.Error<EpisodeSource>(500, "Error creating episode source");
            }
        }

        public async Task<ResponseDto<EpisodeSource>> UpsertFromVendorAsync(UpsertEpisodeSourceFromVendorRequest request, CancellationToken ct)
        {
            _logger.LogInformation("UpsertFromVendor(Episode) ep:{EpisodeId} type:{Type} vendor:{VendorId}",
                request.EpisodeId, request.SourceType, request.SourceId);

            if (string.IsNullOrWhiteSpace(request.SourceUrl))
                return ResponseConst.Error<EpisodeSource>(400, "SourceUrl is required");
            if (string.IsNullOrWhiteSpace(request.SourceType))
                return ResponseConst.Error<EpisodeSource>(400, "SourceType is required");

            // Tận dụng CreateEpisodeSource để gom logic upsert
            var dto = new CreateEpisodeSourceRequest
            {
                episodeID = request.EpisodeId,
                sourceName = request.SourceName,
                sourceType = request.SourceType,
                sourceUrl = request.SourceUrl,
                sourceId = request.SourceId,
                quality = request.Quality,
                language = request.Language,
                isVipOnly = request.IsVipOnly,
                isActive = request.IsActive
            };
            return await CreateEpisodeSource(dto, ct);
        }
        public async Task<ResponseDto<EpisodeSource>> UpdateEpisodeSource(UpdateEpisodeSourceRequest request, CancellationToken ct)
        {
                       _logger.LogInformation("Updating episode source with ID: {EpisodeSourceID}", request.episodeSourceID);
            try
            {
                var existingSource = await _episodeSourceRepository.GetByIdAsync(request.episodeSourceID, ct);
                if (existingSource == null)
                {
                    _logger.LogWarning("Episode source with ID: {EpisodeSourceID} not found", request.episodeSourceID);
                    return ResponseConst.Error<EpisodeSource>(404, "Episode source not found");
                }
                existingSource.quality = request.quality;
                existingSource.sourceName = request.sourceName;
                existingSource.sourceType = request.sourceType;
                existingSource.sourceUrl = request.sourceUrl;
                existingSource.language = request.language;
                existingSource.isVipOnly = request.isVipOnly;
                existingSource.isActive = request.isActive;
                existingSource.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeSourceRepository.UpdateAsync(existingSource);
                    return existingSource;
                }, ct: ct);
                _logger.LogInformation("Episode source updated successfully with ID: {EpisodeSourceID}", existingSource.episodeSourceID);
                return ResponseConst.Success("Episode source updated successfully", existingSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating episode source with ID: {EpisodeSourceID}", request.episodeSourceID);
                return ResponseConst.Error<EpisodeSource>(500, "An error occurred while updating the episode source");
            }
        }
        public async Task<ResponseDto<bool>> DeleteEpisodeSource(int episodeSourceID, CancellationToken ct)
        {
                       _logger.LogInformation("Deleting episode source with ID: {EpisodeSourceID}", episodeSourceID);
            try
            {
                var existingSource = await _episodeSourceRepository.GetByIdAsync(episodeSourceID, ct);
                if (existingSource == null)
                {
                    _logger.LogWarning("Episode source with ID: {EpisodeSourceID} not found", episodeSourceID);
                    return ResponseConst.Error<bool>(404, "Episode source not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeSourceRepository.RemoveAsync(existingSource.episodeSourceID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Episode source deleted successfully with ID: {EpisodeSourceID}", episodeSourceID);
                return ResponseConst.Success("Episode source deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting episode source with ID: {EpisodeSourceID}", episodeSourceID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the episode source");
            }
        }
        public async Task<ResponseDto<EpisodeSource>> GetEpisodeSourceByID(int episodeSourceID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode source with ID: {EpisodeSourceID}", episodeSourceID);
            try
            {
                var episodeSource = await _episodeSourceRepository.GetByIdAsync(episodeSourceID, ct);
                if (episodeSource == null)
                {
                    _logger.LogWarning("Episode source with ID: {EpisodeSourceID} not found", episodeSourceID);
                    return ResponseConst.Error<EpisodeSource>(404, "Episode source not found");
                }
                _logger.LogInformation("Episode source retrieved successfully with ID: {EpisodeSourceID}", episodeSourceID);
                return ResponseConst.Success("Episode source retrieved successfully", episodeSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode source with ID: {EpisodeSourceID}", episodeSourceID);
                return ResponseConst.Error<EpisodeSource>(500, "An error occurred while retrieving the episode source");
            }
        }

        public async Task<ResponseDto<List<EpisodeSource>>> GetEpisodeSourcesByEpisodeID(int episodeID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode sources for episodeID: {EpisodeID}", episodeID);
            try
            {
                var episodeSources = await _episodeSourceRepository.GetAllByEpisodeIDAsync(episodeID, ct);
                if (episodeSources == null || !episodeSources.Any())
                {
                    _logger.LogWarning("No episode sources found for episodeID: {EpisodeID}", episodeID);
                    return ResponseConst.Error<List<EpisodeSource>>(404, "No episode sources found for the specified episode");
                }
                _logger.LogInformation("Successfully retrieved {Count} episode sources for episodeID: {EpisodeID}", episodeSources.Count, episodeID);
                return ResponseConst.Success("Successfully retrieved episode sources", episodeSources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode sources for episodeID: {EpisodeID}", episodeID);
                return ResponseConst.Error<List<EpisodeSource>>(500, "An error occurred while retrieving the episode sources");
            }
        }
        

    }
}
