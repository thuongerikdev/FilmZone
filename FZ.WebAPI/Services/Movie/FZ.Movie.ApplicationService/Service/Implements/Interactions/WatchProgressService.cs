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
    public class WatchProgressService : MovieServiceBase , IWatchProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWatchProgressRepository _watchProgressRepository;
        public WatchProgressService(IWatchProgressRepository watchProgressRepository, IUnitOfWork unitOfWork, ILogger<WatchProgressService> logger) : base(logger)
        {
            _watchProgressRepository = watchProgressRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<WatchProgress>> CreateWatchProgress(CreateWatchProgressRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new watch progress for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
            try
            {
                WatchProgress newWatchProgress = new WatchProgress
                {
                    userID = request.userID,
                    movieID = request.movieID,
                    sourceID = request.sourceID,
                    durationSeconds = request.durationSeconds,
                    positionSeconds = request.positionSeconds,
                    lastWatchedAt = DateTime.UtcNow,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _watchProgressRepository.AddAsync(newWatchProgress, cancellationToken);
                    return newWatchProgress;
                }, ct: ct);
                _logger.LogInformation("Watch progress created successfully with ID: {WatchProgressID}", newWatchProgress.watchProgressID);
                return ResponseConst.Success("Watch progress created successfully", newWatchProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating watch progress for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
                return ResponseConst.Error<WatchProgress>(500, "An error occurred while creating the watch progress");
            }
        }
        public async Task<ResponseDto<WatchProgress>> UpdateWatchProgress(UpdateWatchProgressRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating watch progress with ID: {WatchProgressID}", request.watchProgressID);
            try
            {
                var existingWatchProgress = await _watchProgressRepository.GetByIdAsync(request.watchProgressID, ct);
                if (existingWatchProgress == null)
                {
                    _logger.LogWarning("Watch progress with ID: {WatchProgressID} not found", request.watchProgressID);
                    return ResponseConst.Error<WatchProgress>(404, "Watch progress not found");
                }
                existingWatchProgress.sourceID = request.sourceID;
                existingWatchProgress.durationSeconds = request.durationSeconds;
                existingWatchProgress.positionSeconds = request.positionSeconds;
                existingWatchProgress.lastWatchedAt = DateTime.UtcNow;
                existingWatchProgress.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _watchProgressRepository.UpdateAsync(existingWatchProgress, cancellationToken);
                    return existingWatchProgress;
                }, ct: ct);
                _logger.LogInformation("Watch progress updated successfully with ID: {WatchProgressID}", existingWatchProgress.watchProgressID);
                return ResponseConst.Success("Watch progress updated successfully", existingWatchProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating watch progress with ID: {WatchProgressID}", request.watchProgressID);
                return ResponseConst.Error<WatchProgress>(500, "An error occurred while updating the watch progress");
            }
        }
        public async Task<ResponseDto<bool>> DeleteWatchProgress(int watchProgressID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting watch progress with ID: {WatchProgressID}", watchProgressID);
            try
            {
                var existingWatchProgress = await _watchProgressRepository.GetByIdAsync(watchProgressID, ct);
                if (existingWatchProgress == null)
                {
                    _logger.LogWarning("Watch progress with ID: {WatchProgressID} not found", watchProgressID);
                    return ResponseConst.Error<bool>(404, "Watch progress not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _watchProgressRepository.RemoveAsync(watchProgressID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Watch progress deleted successfully with ID: {WatchProgressID}", watchProgressID);
                return ResponseConst.Success("Watch progress deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting watch progress with ID: {WatchProgressID}", watchProgressID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the watch progress");
            }
        }
        public async Task<ResponseDto<WatchProgress>> GetWatchProgressByID(int watchProgressID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving watch progress with ID: {WatchProgressID}", watchProgressID);
            try
            {
                var watchProgress = await _watchProgressRepository.GetByIdAsync(watchProgressID, ct);
                if (watchProgress == null)
                {
                    _logger.LogWarning("Watch progress with ID: {WatchProgressID} not found", watchProgressID);
                    return ResponseConst.Error<WatchProgress>(404, "Watch progress not found");
                }
                _logger.LogInformation("Successfully retrieved watch progress with ID: {WatchProgressID}", watchProgressID);
                return ResponseConst.Success("Successfully retrieved the watch progress", watchProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving watch progress with ID: {WatchProgressID}", watchProgressID);
                return ResponseConst.Error<WatchProgress>(500, "An error occurred while retrieving the watch progress");
            }
        }
        public async Task<ResponseDto<List<WatchProgress>>> GetWatchProgressByUserID(int userID, CancellationToken ct)
        {
              _logger.LogInformation("Retrieving watch progress for userID: {UserID}", userID);
            try
            {
                var watchProgressList = await _watchProgressRepository.GetAllByUserIdAsync(userID, ct);
                if (watchProgressList == null || !watchProgressList.Any())
                {
                    _logger.LogWarning("No watch progress found for userID: {UserID}", userID);
                    return ResponseConst.Error<List<WatchProgress>>(404, "No watch progress found for the user");
                }
                _logger.LogInformation("Successfully retrieved watch progress for userID: {UserID}", userID);
                return ResponseConst.Success("Successfully retrieved the watch progress", watchProgressList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving watch progress for userID: {UserID}", userID);
                return ResponseConst.Error<List<WatchProgress>>(500, "An error occurred while retrieving the watch progress");
            }
        }
        public async Task<ResponseDto<List<WatchProgress>>> GetWatchProgressByMovieID(int movieID, CancellationToken ct)
        {
              _logger.LogInformation("Retrieving watch progress for movieID: {MovieID}", movieID);
            try
            {
                var watchProgressList = await _watchProgressRepository.GetAllByMovieIDAsync(movieID, ct);
                if (watchProgressList == null || !watchProgressList.Any())
                {
                    _logger.LogWarning("No watch progress found for movieID: {MovieID}", movieID);
                    return ResponseConst.Error<List<WatchProgress>>(404, "No watch progress found for the movie");
                }
                _logger.LogInformation("Successfully retrieved watch progress for movieID: {MovieID}", movieID);
                return ResponseConst.Success("Successfully retrieved the watch progress", watchProgressList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving watch progress for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<WatchProgress>>(500, "An error occurred while retrieving the watch progress");
            }
        }
    }
}
