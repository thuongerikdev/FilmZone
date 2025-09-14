using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.People;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.People;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.People
{
    public class MoviePersonService : MovieServiceBase , IMoviePersonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMoviePersonRepository _moviePersonRepository;
        public MoviePersonService(IUnitOfWork unitOfWork, IMoviePersonRepository moviePersonRepository, ILogger<MoviePersonService> logger) : base(logger)
        {
            _unitOfWork = unitOfWork;
            _moviePersonRepository = moviePersonRepository;
        }
        public async Task<ResponseDto<MoviePerson>> CreateMoviePerson(CreateMoviePersonRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new movie person with ", request.movieID);
            try
            {
                var existingPerson = await _moviePersonRepository.GetByMovieAndPersonIdAsync(request.movieID , request.personID, ct);
                if (existingPerson != null)
                {
                    _logger.LogWarning("Movie person with name: {Name} already exists", request.movieID);
                    return ResponseConst.Error<MoviePerson>(400, "Movie person with the same  already exists");
                }
                MoviePerson newPerson = new MoviePerson
                {
                    movieID = request.movieID,
                    personID = request.personID,
                    role = request.role,
                    characterName = request.characterName,
                    creditOrder = request.creditOrder,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _moviePersonRepository.AddAsync(newPerson, cancellationToken);
                    return newPerson;
                }, ct: ct);
                _logger.LogInformation("Movie person created successfully with ID: {PersonID}", newPerson.personID);
                return ResponseConst.Success("Movie person created successfully", newPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating movie person with", request.movieID);
                return ResponseConst.Error<MoviePerson>(500, "An error occurred while creating the movie person");
            }
        }
        public async Task<ResponseDto<bool>> DeleteMoviePerson(int moviePersonID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting movie person with ID: {MoviePersonID}", moviePersonID);
            try
            {
                var existingMoviePerson = await _moviePersonRepository.GetByIdAsync(moviePersonID, ct);
                if (existingMoviePerson == null)
                {
                    _logger.LogWarning("Movie person with ID: {MoviePersonID} not found", moviePersonID);
                    return ResponseConst.Error<bool>(404, "Movie person not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _moviePersonRepository.RemoveAsync(moviePersonID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Movie person with ID: {MoviePersonID} deleted successfully", moviePersonID);
                return ResponseConst.Success("Movie person deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting movie person with ID: {MoviePersonID}", moviePersonID);
                return ResponseConst.Error<bool>(500, "An error occurred while deleting the movie person");
            }

        }
        public async Task<ResponseDto<List<Person>>> GetCreditsByMovieID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving credits for movieID: {MovieID}", movieID);
            try
            {
                var credits = await _moviePersonRepository.GetCreditsByMovieIdAsync(movieID, ct);
                if (credits == null || credits.Count == 0)
                {
                    _logger.LogWarning("No credits found for movieID: {MovieID}", movieID);
                    return ResponseConst.Error<List<Person>>(404, "No credits found for the specified movie");
                }
                _logger.LogInformation("Successfully retrieved {Count} credits for movieID: {MovieID}", credits.Count, movieID);
                return ResponseConst.Success("Successfully retrieved the credits", credits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving credits for movieID: {MovieID}", movieID);
                return ResponseConst.Error<List<Person>>(500, "An error occurred while retrieving the credits");
            }
        }
        public async Task<ResponseDto<List<Movies>>> GetMoviesByPersonID(int personID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movies for personID: {PersonID}", personID);
            try
            {
                var movies = await _moviePersonRepository.GetMoviesByPersonID(personID, ct);
                if (movies == null || movies.Count == 0)
                {
                    _logger.LogWarning("No movies found for personID: {PersonID}", personID);
                    return ResponseConst.Error<List<Movies>>(404, "No movies found for the specified person");
                }
                _logger.LogInformation("Successfully retrieved {Count} movies for personID: {PersonID}", movies.Count, personID);
                return ResponseConst.Success("Successfully retrieved the movies", movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movies for personID: {PersonID}", personID);
                return ResponseConst.Error<List<Movies>>(500, "An error occurred while retrieving the movies");
            }
        }
        public async Task<ResponseDto<List<MoviePerson>>> GetMoviePersonsByPersonID(int personID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movie persons for personID: {PersonID}", personID);
            try
            {
                var moviePersons = await _moviePersonRepository.GetAllByPersonIdAsync(personID, ct);
                if (moviePersons == null || moviePersons.Count == 0)
                {
                    _logger.LogWarning("No movie persons found for personID: {PersonID}", personID);
                    return ResponseConst.Error<List<MoviePerson>>(404, "No movie persons found for the specified person");
                }
                _logger.LogInformation("Successfully retrieved {Count} movie persons for personID: {PersonID}", moviePersons.Count, personID);
                return ResponseConst.Success("Successfully retrieved the movie persons", moviePersons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movie persons for personID: {PersonID}", personID);
                return ResponseConst.Error<List<MoviePerson>>(500, "An error occurred while retrieving the movie persons");
            }
        }
    }
}
