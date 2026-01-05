using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Interactions;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Interactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Interactions
{
    public class EpisodeWatchProgressService : MovieServiceBase, IEpisodeWatchProgressService
    {
        private readonly IEpisodeWatchProgressRepository _episodeWatchProgressRepository;
        private readonly IUnitOfWork _unitOfWork;
        public EpisodeWatchProgressService(IEpisodeWatchProgressRepository episodeWatchProgressRepository, IUnitOfWork unitOfWork, ILogger<EpisodeWatchProgressService> logger) : base(logger)
        {
            _episodeWatchProgressRepository = episodeWatchProgressRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<EpisodeWatchProgress>> CreateEpisodeWatchProgress(CreateEpisodeWatchProgressRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new episode watch progress for episodeID: {EpisodeID} by userID: {UserID}", request.episodeID, request.userID);
            try
            {
                EpisodeWatchProgress newWatchProgress = new EpisodeWatchProgress
                {
                    userID = request.userID,
                    episodeID = request.episodeID,
                    episodeSourceID = request.episodeSourceID,
                    positionSeconds = request.positionSeconds,
                    durationSeconds = request.durationSeconds,
                    lastWatchedAt = DateTime.UtcNow,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeWatchProgressRepository.AddAsync(newWatchProgress, cancellationToken);
                    return newWatchProgress;
                }, ct: ct);
                _logger.LogInformation("Episode watch progress created successfully with ID: {WatchProgressID}", newWatchProgress.episodeWatchProgressID);
                return ResponseConst.Success("Episode watch progress created successfully", newWatchProgress);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating episode watch progress for episodeID: {EpisodeID} by userID: {UserID}", request.episodeID, request.userID);
                return ResponseConst.Error<EpisodeWatchProgress>(500, "An error occurred while creating the episode watch progress");
            }
        }
        public async Task<ResponseDto<EpisodeWatchProgress>> UpdateEpisodeWatchProgress(UpdateEpisodeWatchProgressRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating episode watch progress with ID: {WatchProgressID}", request.episodeWatchProgressID);
            try
            {
                var existingWatchProgress = await _episodeWatchProgressRepository.GetByIdAsync(request.episodeWatchProgressID, ct);
                if (existingWatchProgress == null)
                {
                    _logger.LogWarning("Episode watch progress with ID: {WatchProgressID} not found", request.episodeWatchProgressID);
                    return ResponseConst.Error<EpisodeWatchProgress>(404, "Episode watch progress not found");
                }
                existingWatchProgress.positionSeconds = request.positionSeconds;
                existingWatchProgress.userID = request.userID;
                existingWatchProgress.durationSeconds = request.durationSeconds;
                existingWatchProgress.episodeSourceID = request.episodeSourceID;
                existingWatchProgress.lastWatchedAt = DateTime.UtcNow;
                existingWatchProgress.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeWatchProgressRepository.UpdateAsync(existingWatchProgress, cancellationToken);
                    return existingWatchProgress;
                }, ct: ct);
                _logger.LogInformation("Episode watch progress updated successfully with ID: {WatchProgressID}", existingWatchProgress.episodeWatchProgressID);
                return ResponseConst.Success("Episode watch progress updated successfully", existingWatchProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating episode watch progress with ID: {WatchProgressID}", request.episodeWatchProgressID);
                return ResponseConst.Error<EpisodeWatchProgress>(500, "An error occurred while updating the episode watch progress");
            }

        }
        public async Task<ResponseDto<bool>> DeleteEpisodeWatchProgress(int episodeWatchProgressID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting episode watch progress with ID: {WatchProgressID}", episodeWatchProgressID);
            try
            {
                var existingWatchProgress = await _episodeWatchProgressRepository.GetByIdAsync(episodeWatchProgressID, ct);
                if (existingWatchProgress == null)
                {
                    _logger.LogWarning("Episode watch progress with ID: {WatchProgressID} not found", episodeWatchProgressID);
                    return ResponseConst.Error<bool>(404, "Episode watch progress not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeWatchProgressRepository.RemoveAsync(episodeWatchProgressID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Episode watch progress deleted successfully with ID: {WatchProgressID}", episodeWatchProgressID);
                return ResponseConst.Success("Episode watch progress deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting episode watch progress with ID: {WatchProgressID}", episodeWatchProgressID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the episode watch progress");
            }
        }
        public async Task<ResponseDto<EpisodeWatchProgress>> GetEpisodeWatchProgressByID(int episodeWatchProgressID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode watch progress with ID: {WatchProgressID}", episodeWatchProgressID);
            try
            {
                var watchProgress = await _episodeWatchProgressRepository.GetByIdAsync(episodeWatchProgressID, ct);
                if (watchProgress == null)
                {
                    _logger.LogWarning("Episode watch progress with ID: {WatchProgressID} not found", episodeWatchProgressID);
                    return ResponseConst.Error<EpisodeWatchProgress>(404, "Episode watch progress not found");
                }
                _logger.LogInformation("Episode watch progress retrieved successfully with ID: {WatchProgressID}", episodeWatchProgressID);
                return ResponseConst.Success("Episode watch progress retrieved successfully", watchProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode watch progress with ID: {WatchProgressID}", episodeWatchProgressID);
                return ResponseConst.Error<EpisodeWatchProgress>(500, "An error occurred while retrieving the episode watch progress");
            }

        }
        public async Task<ResponseDto<List<EpisodeWatchProgress>>> GetEpisodeWatchProgressByUserID(int userID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode watch progresses for userID: {UserID}", userID);
            try
            {
                var watchProgresses = await _episodeWatchProgressRepository.GetAllByUserIdAsync(userID, ct);
                _logger.LogInformation("Episode watch progresses retrieved successfully for userID: {UserID}", userID);
                return ResponseConst.Success("Episode watch progresses retrieved successfully", watchProgresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode watch progresses for userID: {UserID}", userID);
                return ResponseConst.Error<List<EpisodeWatchProgress>>(500, "An error occurred while retrieving the episode watch progresses");
            }
        }
        public async Task<ResponseDto<List<EpisodeWatchProgress>>> GetEpisodeWatchProgressByEpisodeID(int episodeID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode watch progresses for episodeID: {EpisodeID}", episodeID);
            try
            {
                var watchProgresses = await _episodeWatchProgressRepository.GetAllByEposodeIDAsync(episodeID, ct);
                _logger.LogInformation("Episode watch progresses retrieved successfully for episodeID: {EpisodeID}", episodeID);
                return ResponseConst.Success("Episode watch progresses retrieved successfully", watchProgresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode watch progresses for episodeID: {EpisodeID}", episodeID);
                return ResponseConst.Error<List<EpisodeWatchProgress>>(500, "An error occurred while retrieving the episode watch progresses");
            }
        }




    }
}
