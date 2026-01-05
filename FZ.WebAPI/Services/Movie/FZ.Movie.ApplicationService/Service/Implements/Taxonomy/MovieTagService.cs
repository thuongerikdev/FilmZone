using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Taxonomy;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Taxonomy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Taxonomy
{
    public class MovieTagService : MovieServiceBase , IMovieTagService
    {
        private readonly IMovieTagRepository _movieTagRepository;
        private readonly IUnitOfWork _unitOfWork;
        public MovieTagService(IMovieTagRepository movieTagRepository, IUnitOfWork unitOfWork, ILogger<MovieServiceBase> logger) : base(logger)
        {
            _movieTagRepository = movieTagRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<MovieTag>> CreateMovieTag(CreateMoiveTagRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new movie tag with name: {Name}", request.movieID);
            try
            {
                var existingTag = await _movieTagRepository.GetByMovieAndTagAsync(request.movieID, request.tagID  ,ct);
                if (existingTag != null)
                {
                    _logger.LogWarning("Movie tag with name: {Name} already exists", request.movieID);
                    return ResponseConst.Error<MovieTag>(400, "Movie tag with the same name already exists");
                }
                MovieTag newTag = new MovieTag
                {
                    movieID = request.movieID,
                    tagID = request.tagID,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,

                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieTagRepository.AddAsync(newTag, cancellationToken);
                    return newTag;
                }, ct: ct);
                _logger.LogInformation("Movie tag created successfully with ID: {MovieTagID}", newTag.movieTagID);
                return ResponseConst.Success("Movie tag created successfully", newTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating movie tag with name: {Name}", request.movieID);
                return ResponseConst.Error<MovieTag>(500, "An error occurred while creating the movie tag");
            }
        }
        public async Task<ResponseDto<MovieTag>> UpdateMovieTag(UpdateMoiveTagRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating movie tag with ID: {MovieTagID}", request.movieID);
            try
            {
                var existingTag = await _movieTagRepository.GetByIdAsync(request.movieTagID, ct);
                if (existingTag == null)
                {
                    _logger.LogWarning("Movie tag with ID: {MovieTagID} not found", request.movieTagID);
                    return ResponseConst.Error<MovieTag>(404, "Movie tag not found");
                }
                existingTag.movieID = request.movieID;
                existingTag.tagID = request.tagID;
                existingTag.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieTagRepository.UpdateAsync(existingTag, cancellationToken);
                    return existingTag;
                }, ct: ct);
                _logger.LogInformation("Movie tag updated successfully with ID: {MovieTagID}", existingTag.movieTagID);
                return ResponseConst.Success("Movie tag updated successfully", existingTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating movie tag with ID: {MovieTagID}", request.movieTagID);
                return ResponseConst.Error<MovieTag>(500, "An error occurred while updating the movie tag");
            }
        }
        public async Task<ResponseDto<bool>> DeleteMovieTag(int movieTagID, CancellationToken ct)
        {
                       _logger.LogInformation("Deleting movie tag with ID: {MovieTagID}", movieTagID);
            try
            {
                var existingTag = await _movieTagRepository.GetByIdAsync(movieTagID, ct);
                if (existingTag == null)
                {
                    _logger.LogWarning("Movie tag with ID: {MovieTagID} not found", movieTagID);
                    return ResponseConst.Error<bool>(404, "Movie tag not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieTagRepository.RemoveAsync(existingTag.movieTagID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Movie tag deleted successfully with ID: {MovieTagID}", movieTagID);
                return ResponseConst.Success("Movie tag deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting movie tag with ID: {MovieTagID}", movieTagID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the movie tag");
            }
        }
        public async Task<ResponseDto<MovieTag>> GetMovieTagByID(int movieTagID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movie tag with ID: {MovieTagID}", movieTagID);
            try
            {
                var movieTag = await _movieTagRepository.GetByIdAsync(movieTagID, ct);
                if (movieTag == null)
                {
                    _logger.LogWarning("Movie tag with ID: {MovieTagID} not found", movieTagID);
                    return ResponseConst.Error<MovieTag>(404, "Movie tag not found");
                }
                _logger.LogInformation("Successfully retrieved movie tag with ID: {MovieTagID}", movieTagID);
                return ResponseConst.Success("Successfully retrieved the movie tag", movieTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movie tag with ID: {MovieTagID}", movieTagID);
                return ResponseConst.Error<MovieTag>(500, "An error occurred while retrieving the movie tag");
            }
        }

        public async Task<ResponseDto<List<MovieTag>>> GetAllMovieTags(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all movie tags");
            try
            {
                var movieTags = await _movieTagRepository.GetAllMovieTagAsync(ct);
                _logger.LogInformation("Successfully retrieved all movie tags, count: {Count}", movieTags.Count);
                return ResponseConst.Success("Successfully retrieved all movie tags", movieTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all movie tags");
                return ResponseConst.Error<List<MovieTag>>(500, "An error occurred while retrieving the movie tags");
            }
        }
         public async Task<ResponseDto<List<Movies>>> GetMoviesByTagIDs(List<int> tagIDs, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movies by tag IDs");
            try
            {
                var movies = await _movieTagRepository.GetMovieByTagID(tagIDs, ct);
                _logger.LogInformation("Successfully retrieved movies by tag IDs, count: {Count}", movies.Count);
                return ResponseConst.Success("Successfully retrieved movies by tag IDs", movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movies by tag IDs");
                return ResponseConst.Error<List<Movies>>(500, "An error occurred while retrieving the movies by tag IDs");
            }
        }
        public async Task<ResponseDto<List<Tag>>> GetTagByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving tags by movie ID");
            try
            {
                var tags = await _movieTagRepository.GetTagByMovieID(movieID, ct);
                _logger.LogInformation("Successfully retrieved tags by movie ID, count: {Count}", tags.Count);
                return ResponseConst.Success("Successfully retrieved tags by movie ID", tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tags by movie ID");
                return ResponseConst.Error<List<Tag>>(500, "An error occurred while retrieving the tags by movie ID");
            }
        }
    }
}
