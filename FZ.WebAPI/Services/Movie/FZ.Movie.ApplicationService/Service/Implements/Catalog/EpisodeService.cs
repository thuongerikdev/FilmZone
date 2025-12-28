using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Catalog;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Catalog
{
    public class EpisodeService : MovieServiceBase, IEpisodeService
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public EpisodeService(ILogger<MovieServiceBase> logger, IEpisodeRepository episode, IUnitOfWork unitOfWork) : base(logger)
        {
            _episodeRepository = episode;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<Episode>> CreateEpisode(CreateEpisodeRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new episode with title: {Title}", request.title);
            try
            {
                var existingEpisode = await _episodeRepository.GetByTitleAsync(request.title, ct);
                if (existingEpisode != null)
                {
                    _logger.LogWarning("Episode with title: {Title} already exists", request.title);
                    return ResponseConst.Error<Episode>(400, "Episode with the same title already exists");
                }
                Episode newEpisode = new Episode
                {
                    title = request.title,
                    description = request.description,
                    seasonNumber = request.seasonNumber,
                    episodeNumber = request.episodeNumber,
                    durationSeconds = request.durationSeconds,
                    synopsis = request.synopsis,

                    releaseDate = request.releaseDate,
                    movieID = request.movieID,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeRepository.AddEpisodeAsync(newEpisode, cancellationToken);
                    return newEpisode;
                }, ct: ct);
                _logger.LogInformation("Episode created successfully with ID: {EpisodeID}", newEpisode.episodeID);
                return ResponseConst.Success("Episode created successfully", newEpisode);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating episode with title: {Title}", request.title);
                return ResponseConst.Error<Episode>(500, "An error occurred while creating the episode");
            }
        }
        public async Task<ResponseDto<Episode>> UpdateEpisode(UpdateEpisodeRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating episode with ID: {EpisodeID}", request.episodeID);
            try
            {
                var existingEpisode = await _episodeRepository.GetByIdAsync(request.episodeID, ct);
                if (existingEpisode == null)
                {
                    _logger.LogWarning("Episode with ID: {EpisodeID} not found", request.episodeID);
                    return ResponseConst.Error<Episode>(404, "Episode not found");
                }
                // Cập nhật các trường của episode
                existingEpisode.title = request.title;
                existingEpisode.description = request.description;
                existingEpisode.seasonNumber = request.seasonNumber;
                existingEpisode.episodeNumber = request.episodeNumber;
                existingEpisode.durationSeconds = request.durationSeconds;
                existingEpisode.synopsis = request.synopsis;
                existingEpisode.releaseDate = request.releaseDate;
                existingEpisode.movieID = request.movieID;


                existingEpisode.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeRepository.UpdateAsync(existingEpisode, cancellationToken);
                    return existingEpisode;
                }, ct: ct);
                _logger.LogInformation("Episode with ID: {EpisodeID} updated successfully", request.episodeID);
                return ResponseConst.Success("Episode updated successfully", existingEpisode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating episode with ID: {EpisodeID}", request.episodeID);
                return ResponseConst.Error<Episode>(500, "An error occurred while updating the episode");
            }
        }
        public async Task<ResponseDto<bool>> DeleteEpisode(int episodeID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting episode with ID: {EpisodeID}", episodeID);
            try
            {
                var existingEpisode = await _episodeRepository.GetByIdAsync(episodeID, ct);
                if (existingEpisode == null)
                {
                    _logger.LogWarning("Episode with ID: {EpisodeID} not found", episodeID);
                    return ResponseConst.Error<bool>(404, "Episode not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _episodeRepository.HardDeleteAsync(episodeID, cancellationToken);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Episode with ID: {EpisodeID} deleted successfully", episodeID);
                return ResponseConst.Success("Episode deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting episode with ID: {EpisodeID}", episodeID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the episode");
            }
        }
        public async Task<ResponseDto<Episode>> GetEpisodeByID(int episodeID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episode with ID: {EpisodeID}", episodeID);
            try
            {
                var episode = await _episodeRepository.GetByIdAsync(episodeID, ct);
                if (episode == null)
                {
                    _logger.LogWarning("Episode with ID: {EpisodeID} not found", episodeID);
                    return ResponseConst.Error<Episode>(404, "Episode not found");
                }
                _logger.LogInformation("Episode with ID: {EpisodeID} retrieved successfully", episodeID);
                return ResponseConst.Success("Episode retrieved successfully", episode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episode with ID: {EpisodeID}", episodeID);
                return ResponseConst.Error<Episode>(500, "An error occurred while retrieving the episode");
            }
        }
        public async Task<ResponseDto<List<Episode>>> GetAllEpisode(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all episodes");
            try
            {
                var episodes = await _episodeRepository.GetAllEpisodeAsync(ct);
                _logger.LogInformation("All episodes retrieved successfully, count: {Count}", episodes.Count);
                return ResponseConst.Success("All episodes retrieved successfully", episodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all episodes");
                return ResponseConst.Error<List<Episode>>(500, "An error occurred while retrieving all episodes");
            }
        }
        public async Task<ResponseDto<List<Episode>>> GetEpisodeByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving episodes for movie ID: {MovieID}", movieID);
            try
            {
                var episodes = await _episodeRepository.GetEpisodesByMovieIdAsync(movieID, ct);
                _logger.LogInformation("Episodes for movie ID: {MovieID} retrieved successfully, count: {Count}", movieID, episodes.Count);
                return ResponseConst.Success("Episodes retrieved successfully", episodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving episodes for movie ID: {MovieID}", movieID);
                return ResponseConst.Error<List<Episode>>(500, "An error occurred while retrieving the episodes");
            }
        }
    }
}