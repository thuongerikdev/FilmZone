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
    public class SavedMovieService : MovieServiceBase, ISavedMovieService
    {
        private readonly ISavedMovieRepository _savedMovieRepository;
        private readonly IUnitOfWork _unitOfWork;
        public SavedMovieService(ISavedMovieRepository savedMovieRepository, IUnitOfWork unitOfWork, ILogger<SavedMovieService> logger) : base(logger)
        {
            _savedMovieRepository = savedMovieRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<SavedMovie>> CreateSavedMovie(CreateSavedMovieRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new saved movie for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
            try
            {
                var existingSavedMovie = await _savedMovieRepository.GetByUserAndMovieIDAsync(request.userID, request.movieID, ct);
                if (existingSavedMovie != null)
                {
                    _logger.LogWarning("Saved movie for movieID: {MovieID} by userID: {UserID} already exists", request.movieID, request.userID);
                    return ResponseConst.Error<SavedMovie>(400, "This movie is already saved by the user");
                }
                SavedMovie newSavedMovie = new SavedMovie
                {
                    userID = request.userID,
                    movieID = request.movieID,
                    createdAt = DateTime.UtcNow,
                   
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _savedMovieRepository.AddAsync(newSavedMovie, cancellationToken);
                    return newSavedMovie;
                }, ct: ct);
                _logger.LogInformation("Saved movie created successfully with ID: {SavedMovieID}", newSavedMovie.savedMovieID);
                return ResponseConst.Success("Saved movie created successfully", newSavedMovie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating saved movie for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
                return ResponseConst.Error<SavedMovie>(500, "An error occurred while creating the saved movie");
            }
        }
        public async Task<ResponseDto<SavedMovie>> UpdateSavedMovie(UpdateSavedMovieRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating saved movie with ID: {SavedMovieID}", request.savedMovieID);
            try
            {
                var existingSavedMovie = await _savedMovieRepository.GetByIdAsync(request.savedMovieID, ct);
                if (existingSavedMovie == null)
                {
                    _logger.LogWarning("Saved movie with ID: {SavedMovieID} not found", request.savedMovieID);
                    return ResponseConst.Error<SavedMovie>(404, "Saved movie not found");
                }
                existingSavedMovie.movieID = request.movieID;
                existingSavedMovie.userID = request.userID;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _savedMovieRepository.UpdateAsync(existingSavedMovie, cancellationToken);
                    return existingSavedMovie;
                }, ct: ct);
                _logger.LogInformation("Saved movie updated successfully with ID: {SavedMovieID}", existingSavedMovie.savedMovieID);
                return ResponseConst.Success("Saved movie updated successfully", existingSavedMovie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating saved movie with ID: {SavedMovieID}", request.savedMovieID);
                return ResponseConst.Error<SavedMovie>(500, "An error occurred while updating the saved movie");
            }
        }
        public async Task<ResponseDto<bool>> DeleteSavedMovie(int savedMovieID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting saved movie with ID: {SavedMovieID}", savedMovieID);
            try
            {
                var existingSavedMovie = await _savedMovieRepository.GetByIdAsync(savedMovieID, ct);
                if (existingSavedMovie == null)
                {
                    _logger.LogWarning("Saved movie with ID: {SavedMovieID} not found", savedMovieID);
                    return ResponseConst.Error<bool>(404, "Saved movie not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _savedMovieRepository.RemoveAsync(existingSavedMovie.movieID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Saved movie deleted successfully with ID: {SavedMovieID}", savedMovieID);
                return ResponseConst.Success("Saved movie deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting saved movie with ID: {SavedMovieID}", savedMovieID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the saved movie");
            }
        }
        public async Task<ResponseDto<SavedMovie>> GetSavedMovieByID(int savedMovieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving saved movie with ID: {SavedMovieID}", savedMovieID);
            try
            {
                var savedMovie = await _savedMovieRepository.GetByIdAsync(savedMovieID, ct);
                if (savedMovie == null)
                {
                    _logger.LogWarning("Saved movie with ID: {SavedMovieID} not found", savedMovieID);
                    return ResponseConst.Error<SavedMovie>(404, "Saved movie not found");
                }
                _logger.LogInformation("Successfully retrieved saved movie with ID: {SavedMovieID}", savedMovieID);
                return ResponseConst.Success("Successfully retrieved the saved movie", savedMovie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving saved movie with ID: {SavedMovieID}", savedMovieID);
                return ResponseConst.Error<SavedMovie>(500, "An error occurred while retrieving the saved movie");
            }
        }
        public async Task<ResponseDto<List<SavedMovie>>> GetSavedMoviesByUserID(int userID, CancellationToken ct)
        {
             _logger.LogInformation("Retrieving saved movies for userID: {UserID}", userID);
            try
            {
                var savedMovies = await _savedMovieRepository.GetAllByUserIdAsync(userID, ct);
                _logger.LogInformation("Successfully retrieved {Count} saved movies for userID: {UserID}", savedMovies.Count, userID);
                return ResponseConst.Success("Successfully retrieved the saved movies", savedMovies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving saved movies for userID: {UserID}", userID);
                return ResponseConst.Error<List<SavedMovie>>(500, "An error occurred while retrieving the saved movies");
            }
        }
        public async Task<ResponseDto<List<SavedMovie>>> GetSavedMoviesByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving saved movies for movieID: {MovieID}", movieID);
            try
            {
                var savedMovies = await _savedMovieRepository.GetAllByMovieIDAsync(movieID, ct);
                _logger.LogInformation("Successfully retrieved {Count} saved movies for movieID: {MovieID}", savedMovies.Count, movieID);
                return ResponseConst.Success("Successfully retrieved the saved movies", savedMovies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving saved movies for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<SavedMovie>>(500, "An error occurred while retrieving the saved movies");
            }
        }

    }
}
