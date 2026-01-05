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
    public class UserRatingService : MovieServiceBase , IUserRatingService
    {
        private readonly IUserRatingRepository _userRatingRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserRatingService(IUserRatingRepository userRatingRepository, ILogger<UserRatingService> logger , IUnitOfWork unitOfWork) : base(logger)
        {
            _userRatingRepository = userRatingRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<UserRating>> CreateUserRating(CreateUserRatingRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new user rating for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
            try
            {
                UserRating newUserRating = new UserRating
                {
                   
                    userID = request.userID,
                    movieID = request.movieID,
                    stars = request.rating,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _userRatingRepository.AddAsync(newUserRating, cancellationToken);
                    return newUserRating;
                }, ct: ct);
                _logger.LogInformation("User rating created successfully with ID: {UserRatingID}", newUserRating.userRatingID);
                return ResponseConst.Success("User rating created successfully", newUserRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user rating for movieID: {MovieID} by userID: {UserID}", request.movieID, request.userID);
                return ResponseConst.Error<UserRating>(500, "An error occurred while creating the user rating");
            }
        }
        public async Task<ResponseDto<UserRating>> UpdateUserRating(UpdateUserRatingRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating user rating with ID: {UserRatingID}", request.userRatingID);
            try
            {
                var existingUserRating = await _userRatingRepository.GetByIdAsync(request.userRatingID, ct);
                if (existingUserRating == null)
                {
                    _logger.LogWarning("User rating with ID: {UserRatingID} not found", request.userRatingID);
                    return ResponseConst.Error<UserRating>(404, "User rating not found");
                }
                existingUserRating.stars= request.rating;
                existingUserRating.updatedAt = DateTime.UtcNow;
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _userRatingRepository.UpdateAsync(existingUserRating, cancellationToken);
                    return existingUserRating;
                }, ct: ct);
                _logger.LogInformation("Successfully updated user rating with ID: {UserRatingID}", request.userRatingID);
                return ResponseConst.Success("Successfully updated the user rating", existingUserRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user rating with ID: {UserRatingID}", request.userRatingID);
                return ResponseConst.Error<UserRating>(500, "An error occurred while updating the user rating");
            }
        }
        public async Task<ResponseDto<bool>> DeleteUserRating(int userRatingID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting user rating with ID: {UserRatingID}", userRatingID);
            try
            {
                var existingUserRating = await _userRatingRepository.GetByIdAsync(userRatingID, ct);
                if (existingUserRating == null)
                {
                    _logger.LogWarning("User rating with ID: {UserRatingID} not found", userRatingID);
                    return ResponseConst.Error<bool>(404, "User rating not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _userRatingRepository.RemoveAsync(userRatingID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Successfully deleted user rating with ID: {UserRatingID}", userRatingID);
                return ResponseConst.Success("Successfully deleted the user rating", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user rating with ID: {UserRatingID}", userRatingID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the user rating");
            }
        }
        public async Task<ResponseDto<UserRating>> GetUserRatingByID(int userRatingID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving user rating with ID: {UserRatingID}", userRatingID);
            try
            {
                var userRating = await _userRatingRepository.GetByIdAsync(userRatingID, ct);
                if (userRating == null)
                {
                    _logger.LogWarning("User rating with ID: {UserRatingID} not found", userRatingID);
                    return ResponseConst.Error<UserRating>(404, "User rating not found");
                }
                _logger.LogInformation("Successfully retrieved user rating with ID: {UserRatingID}", userRatingID);
                return ResponseConst.Success("Successfully retrieved the user rating", userRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user rating with ID: {UserRatingID}", userRatingID);
                return ResponseConst.Error<UserRating>(500, "An error occurred while retrieving the user rating");
            }

        }

        public async Task<ResponseDto<List<UserRating>>> GetUserRatingsByUserID(int userID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving user ratings for userID: {UserID}", userID);
            try
            {
                var userRatings = await _userRatingRepository.GetAllByUserIdAsync(userID, ct);
                if (userRatings == null || userRatings.Count == 0)
                {
                    _logger.LogWarning("No user ratings found for userID: {UserID}", userID);
                    return ResponseConst.Error<List<UserRating>>(404, "No user ratings found for the specified user");
                }
                _logger.LogInformation("Successfully retrieved {Count} user ratings for userID: {UserID}", userRatings.Count, userID);
                return ResponseConst.Success("Successfully retrieved the user ratings", userRatings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user ratings for userID: {UserID}", userID);
                return ResponseConst.Error<List<UserRating>>(500, "An error occurred while retrieving the user ratings");
            }
        }
        public async Task<ResponseDto<List<UserRating>>> GetUserRatingsByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving user ratings for movieID: {MovieID}", movieID);
            try
            {
                var userRatings = await _userRatingRepository.GetAllByMovieIDAsync(movieID, ct);
                if (userRatings == null || userRatings.Count == 0)
                {
                    _logger.LogWarning("No user ratings found for movieID: {MovieID}", movieID);
                    return ResponseConst.Error<List<UserRating>>(404, "No user ratings found for the specified movie");
                }
                _logger.LogInformation("Successfully retrieved {Count} user ratings for movieID: {MovieID}", userRatings.Count, movieID);
                return ResponseConst.Success("Successfully retrieved the user ratings", userRatings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user ratings for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<UserRating>>(500, "An error occurred while retrieving the user ratings");
            }
        }




    }
}
