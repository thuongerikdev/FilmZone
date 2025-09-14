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
    public class MovieSourceService : MovieServiceBase , IMovieSourceService
    {
        private readonly IMovieSourceRepository _movieSourceRepository;
        private readonly IUnitOfWork _unitOfWork;
        public MovieSourceService(IMovieSourceRepository movieSourceRepository, IUnitOfWork unitOfWork , ILogger<MovieSourceService> logger) : base(logger)
        {
            _movieSourceRepository = movieSourceRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<MovieSource>> CreateMovieSource(CreateMovieSourceRequest request, CancellationToken ct)
        {
            _logger.LogInformation("CreateMovieSource mv:{MovieID} type:{Type} quality:{Quality}",
                request.movieID, request.sourceType, request.quality);

            try
            {
                if (string.IsNullOrWhiteSpace(request.sourceUrl))
                    return ResponseConst.Error<MovieSource>(400, "sourceUrl is required");
                if (string.IsNullOrWhiteSpace(request.sourceName))
                    return ResponseConst.Error<MovieSource>(400, "sourceName is required");

                var normalizedType = request.sourceType?.Trim().ToLowerInvariant() ?? "custom";
                var normalizedId = request.sourceId?.Trim();

                MovieSource? existing = null;
                if (!string.IsNullOrEmpty(normalizedId))
                {
                    existing = await _movieSourceRepository.GetByCompositeKeyAsync(
                        request.movieID, normalizedType, normalizedId, request.language, request.quality, ct);
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
                        await _movieSourceRepository.UpdateAsync(existing, token);
                        return true;
                    }, ct: ct);

                    _logger.LogInformation("MovieSource upserted (updated) id:{Id}", existing.movieSourceID);
                    return ResponseConst.Success("Movie source updated", existing);
                }
                else
                {
                    var newMovieSource = new MovieSource
                    {
                        movieID = request.movieID,
                        sourceName = request.sourceName,
                        sourceType = normalizedType,
                        sourceUrl = request.sourceUrl,
                        sourceID = normalizedId, // string?
                        isActive = request.isActive,
                        isVipOnly = request.isVipOnly,
                        quality = request.quality,
                        language = request.language,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                    };

                    await _unitOfWork.ExecuteInTransactionAsync(async token =>
                    {
                        await _movieSourceRepository.Add(newMovieSource, token);
                        return newMovieSource;
                    }, ct: ct);

                    _logger.LogInformation("MovieSource created id:{Id}", newMovieSource.movieSourceID);
                    return ResponseConst.Success("Movie source created successfully", newMovieSource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateMovieSource error mv:{MovieID}", request.movieID);
                return ResponseConst.Error<MovieSource>(500, "Error creating movie source");
            }
        }

        public async Task<ResponseDto<MovieSource>> UpsertFromVendorAsync(UpsertMovieSourceFromVendorRequest request, CancellationToken ct)
        {
            _logger.LogInformation("UpsertFromVendor(Movie) mv:{MovieId} type:{Type} vendor:{VendorId}",
                request.MovieId, request.SourceType, request.SourceId);

            if (string.IsNullOrWhiteSpace(request.SourceUrl))
                return ResponseConst.Error<MovieSource>(400, "SourceUrl is required");
            if (string.IsNullOrWhiteSpace(request.SourceType))
                return ResponseConst.Error<MovieSource>(400, "SourceType is required");

            var dto = new CreateMovieSourceRequest
            {
                movieID = request.MovieId,
                sourceName = request.SourceName,
                sourceType = request.SourceType,
                sourceUrl = request.SourceUrl,
                sourceId = request.SourceId,   // string!
                isActive = request.IsActive,
                isVipOnly = request.IsVipOnly,
                quality = request.Quality,
                language = request.Language
            };
            return await CreateMovieSource(dto, ct);
        }
        public async Task<ResponseDto<MovieSource>> UpdateMovieSource(UpdateMovieSourceRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating movie source");
            try
            {
                var existingMovieSource = await _movieSourceRepository.GetByIdAsync(request.sourceID, ct);
                if (existingMovieSource == null)
                {
                    _logger.LogWarning("Movie source with ID: {MovieSourceID} not found", request.sourceID);
                    return ResponseConst.Error<MovieSource>(404, "Movie source not found");
                }
                existingMovieSource.sourceName = request.sourceName;
                existingMovieSource.sourceType = request.sourceType;
                existingMovieSource.sourceUrl = request.sourceUrl;
                existingMovieSource.sourceID = request.sourceId;
                existingMovieSource.isActive = request.isActive;
                existingMovieSource.isVipOnly = request.isVipOnly;
                existingMovieSource.quality = request.quality;
                existingMovieSource.language = request.language;
                existingMovieSource.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieSourceRepository.UpdateAsync(existingMovieSource);
                    return existingMovieSource;
                }, ct: ct);
                _logger.LogInformation("Movie source updated successfully with ID: {MovieSourceID}", existingMovieSource.movieSourceID);
                return ResponseConst.Success("Movie source updated successfully", existingMovieSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating movie source with ID: {MovieSourceID}", request.sourceID);
                return ResponseConst.Error<MovieSource>(500, "An error occurred while updating the movie source");
            }
        }
        public async Task<ResponseDto<bool>> DeleteMovieSource(int movieSourceID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting movie source with ID: {MovieSourceID}", movieSourceID);
            try
            {
                var existingMovieSource = await _movieSourceRepository.GetByIdAsync(movieSourceID, ct);
                if (existingMovieSource == null)
                {
                    _logger.LogWarning("Movie source with ID: {MovieSourceID} not found", movieSourceID);
                    return ResponseConst.Error<bool>(404, "Movie source not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieSourceRepository.RemoveAsync(existingMovieSource.movieID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Movie source with ID: {MovieSourceID} deleted successfully", movieSourceID);
                return ResponseConst.Success("Movie source deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting movie source with ID: {MovieSourceID}", movieSourceID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the movie source");
            }
        }
        public async Task<ResponseDto<MovieSource>> GetMovieSourceByID(int movieSourceID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movie source with ID: {MovieSourceID}", movieSourceID);
            try
            {
                var movieSource = await _movieSourceRepository.GetByIdAsync(movieSourceID, ct);
                if (movieSource == null)
                {
                    _logger.LogWarning("Movie source with ID: {MovieSourceID} not found", movieSourceID);
                    return ResponseConst.Error<MovieSource>(404, "Movie source not found");
                }
                _logger.LogInformation("Successfully retrieved movie source with ID: {MovieSourceID}", movieSourceID);
                return ResponseConst.Success("Successfully retrieved the movie source", movieSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movie source with ID: {MovieSourceID}", movieSourceID);
                return ResponseConst.Error<MovieSource>(500, "An error occurred while retrieving the movie source");
            }
        }
        public async Task<ResponseDto<List<MovieSource>>> GetMovieSourcesByMovieID(int movieID, CancellationToken ct)
        {
                       _logger.LogInformation("Retrieving movie sources for movieID: {MovieID}", movieID);
            try
            {
                var movieSources = await _movieSourceRepository.GetByMovieID(movieID, ct);
                if (movieSources == null || !movieSources.Any())
                {
                    _logger.LogWarning("No movie sources found for movieID: {MovieID}", movieID);
                    return ResponseConst.Error<List<MovieSource>>(404, "No movie sources found for the specified movie");
                }
                _logger.LogInformation("Successfully retrieved {Count} movie sources for movieID: {MovieID}", movieSources.Count, movieID);
                return ResponseConst.Success("Successfully retrieved the movie sources", movieSources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movie sources for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<MovieSource>>(500, "An error occurred while retrieving the movie sources");
            }
        }

    }
}
