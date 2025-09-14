using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Media;
using FZ.Movie.Domain.People;
using FZ.Movie.Domain.Taxonomy;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Infrastructure;
using FZ.Movie.Infrastructure.Repository;
using FZ.Movie.Infrastructure.Repository.Catalog;
using FZ.Movie.Infrastructure.Repository.Media;
using FZ.Movie.Infrastructure.Repository.People;
using FZ.Movie.Infrastructure.Repository.Taxonomy;
using FZ.Shared.ApplicationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Implements.Catalog
{
    public class MoviesService : MovieServiceBase, IMoviesService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMovieTagRepository _movieTagRepository;
        private readonly IMoviePersonRepository _moviePersonRepository;
        private readonly IMovieImageRepository _movieImageRepository;
        public MoviesService(
            ILogger<MovieServiceBase> logger , 
            IMovieRepository movie , 
            IUnitOfWork unitOfWork , 
            ICloudinaryService cloudinaryService,
            IMovieTagRepository movieTagRepository,
            IMoviePersonRepository movieperson,
            IMovieImageRepository movieImageRepository

            ) : base(logger)
        {
            _movieRepository = movie;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _movieTagRepository = movieTagRepository;
            _moviePersonRepository = movieperson;
            _movieImageRepository = movieImageRepository;
        }
        public async Task<ResponseDto<Movies>> CreateMovie(CreateMoviesRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new movie with slug: {Slug}", request.slug);
            try
            {
                var existingMovie = await _movieRepository.GetBySlugAsync(request.slug, ct);
                if (existingMovie != null)
                {
                    _logger.LogWarning("Movie with slug: {Slug} already exists", request.slug);
                    return ResponseConst.Error<Movies>(400, "Movie with the same slug already exists");
                }
                var image = await _cloudinaryService.UploadImageAsync(request.image);

                Movies newMovie = new Movies
                {
                    title = request.title,
                    slug = request.slug,
                    description = request.description,
                    regionID = request.regionID,
                    releaseDate = request.releaseDate,
                    originalTitle = request.originalTitle,
                    movieType = request.movieType,
                    status = request.status,
                    durationSeconds = request.durationSeconds,
                    totalSeasons = request.totalSeasons,
                    totalEpisodes = request.totalEpisodes,
                    year = request.year,
                    rated = request.rated,
                    popularity = request.popularity,
                    image = image,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,

                };
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieRepository.AddAsync(newMovie, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);



                    foreach (var tagID in request.tagIDs.Distinct())
                    {
                        var movieTag = new MovieTag
                        {
                            movieID = newMovie.movieID,
                            tagID = tagID
                        };
                        await _movieTagRepository.AddAsync(movieTag, cancellationToken);
                    }
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    foreach (var person in request.person)
                    {
                        var moviePerson = new MoviePerson
                        {
                            movieID = newMovie.movieID,
                            personID = person.personID,
                            role = person.role,
                            characterName = person.characterName,
                            creditOrder = person.creditOrder

                        };
                        await _moviePersonRepository.AddAsync(moviePerson, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    foreach(var image in request.movieImages)
                    {
                        var movieImage = new MovieImage
                        {
                            movieID = newMovie.movieID,
                            ImageUrl = await _cloudinaryService.UploadImageAsync(image.image)
                        };
                        await _movieImageRepository.AddMovieImageAsync(movieImage, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }



                    return true;
                }, ct: ct);
                _logger.LogInformation("Successfully created a new movie with slug: {Slug}", request.slug);
                return ResponseConst.Success("Successfully created a new movie with slug", newMovie);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new movie with slug: {Slug}", request.slug);

                return ResponseConst.Error<Movies>(500, "Error occurred while creating a new movie");
            }

        }
        public async Task<ResponseDto<Movies>> UpdateMovie(UpdateMoviesRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Updating movie with ID: {MovieID}", request.movieID);

            try
            {
                var movie = await _movieRepository.GetTrackedAsync(request.movieID, ct);
                if (movie is null)
                {
                    _logger.LogWarning("Movie with ID: {MovieID} not found", request.movieID);
                    return ResponseConst.Error<Movies>(404, "Movie not found");
                }

                // Lưu poster cũ để xóa sau khi commit
                var oldPoster = movie.image;

                // === Poster (ảnh chính) ===
                if (request.image != null)
                {
                    var newPosterUrl = await _cloudinaryService.UploadImageAsync(request.image);
                    movie.image = newPosterUrl;
                }

                // === Map fields cơ bản ===
                movie.title = request.title;
                movie.slug = request.slug;
                movie.description = request.description;
                movie.releaseDate = request.releaseDate;
                movie.originalTitle = request.originalTitle;
                movie.movieType = request.movieType;
                movie.status = request.status;
                movie.durationSeconds = request.durationSeconds;
                movie.totalSeasons = request.totalSeasons;
                movie.totalEpisodes = request.totalEpisodes;
                movie.year = request.year;
                movie.rated = request.rated;
                movie.popularity = request.popularity;
                movie.regionID = request.regionID;
                movie.updatedAt = DateTime.UtcNow;

                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    // --- Movie ---
                    await _movieRepository.UpdateAsync(movie);

                    // --- TAGs ---
                    {
                        var reqTagIds = new HashSet<int>((request.tagIDs ?? Enumerable.Empty<int>()).Distinct());
                        var existingTags = await _movieTagRepository.GetByMovieIDsync(movie.movieID, cancellationToken)
                                           ?? new List<MovieTag>();
                        var existingTagIds = new HashSet<int>(existingTags.Select(t => t.tagID));

                        // Xóa tag cũ
                        foreach (var mt in existingTags.Where(t => !reqTagIds.Contains(t.tagID)))
                            await _movieTagRepository.RemoveAsync(mt.movieTagID);

                        // Thêm tag mới
                        foreach (var newTagId in reqTagIds.Where(id => !existingTagIds.Contains(id)))
                            await _movieTagRepository.AddAsync(new MovieTag { movieID = movie.movieID, tagID = newTagId }, cancellationToken);
                    }

                    // --- PERSONs ---
                    {
                        var reqPersons = request.person ;
                        var reqPersonMap = reqPersons.ToDictionary(p => p.personID, p => p);

                        var existingPersons = await _moviePersonRepository.GetMoviePersonByMovieID(movie.movieID, cancellationToken)
                                             ?? new List<MoviePerson>();
                        var existingIds = new HashSet<int>(existingPersons.Select(p => p.personID));
                        var requestedIds = new HashSet<int>(reqPersons.Select(p => p.personID));

                        // Xóa person cũ
                        foreach (var old in existingPersons.Where(p => !requestedIds.Contains(p.personID)))
                            await _moviePersonRepository.RemoveAsync(old.moviePersonID);

                        // Cập nhật person giữ lại
                        foreach (var keep in existingPersons.Where(p => requestedIds.Contains(p.personID)))
                        {
                            var src = reqPersonMap[keep.personID];
                            keep.role = src.role;
                            keep.characterName = src.characterName;
                            keep.creditOrder = src.creditOrder;
                            await _moviePersonRepository.UpdateAsync(keep, cancellationToken);
                        }

                        // Thêm person mới
                        foreach (var add in reqPersons.Where(p => !existingIds.Contains(p.personID)))
                        {
                            await _moviePersonRepository.AddAsync(new MoviePerson
                            {
                                movieID = movie.movieID,
                                personID = add.personID,
                                role = add.role,
                                characterName = add.characterName,
                                creditOrder = add.creditOrder
                            }, cancellationToken);
                        }
                    }

                    // --- MovieImages ---
                    {
                        var existingImages = await _movieImageRepository.GetByMovieID(movie.movieID, cancellationToken)

                                              ?? new List<MovieImage>();
                        var requestImages = request.MovieImage;
                        var requestedIds = new HashSet<int>(requestImages.Where(x => x.movieImageID > 0)
                                                                         .Select(x => x.movieImageID));

                        // Xóa ảnh cũ không còn trong request
                        foreach (var old in existingImages.Where(e => !requestedIds.Contains(e.movieImageID)))
                        {
                            if (!string.IsNullOrWhiteSpace(old.ImageUrl))
                                await _cloudinaryService.DeleteImageAsync(old.ImageUrl);

                            await _movieImageRepository.RemoveAsync(old.movieImageID);
                        }

                        // Cập nhật ảnh giữ lại (nếu có file mới thì replace)
                        foreach (var keep in requestImages.Where(x => x.movieImageID > 0))
                        {
                            var ex = existingImages.FirstOrDefault(e => e.movieImageID == keep.movieImageID);
                            if (ex == null) continue;

                            if (keep.image != null)
                            {
                                if (!string.IsNullOrWhiteSpace(ex.ImageUrl))
                                    await _cloudinaryService.DeleteImageAsync(ex.ImageUrl);

                                ex.ImageUrl = await _cloudinaryService.UploadImageAsync(keep.image);
                                await _movieImageRepository.AddMovieImageAsync(ex, cancellationToken);
                            }
                        }

                        // Thêm ảnh mới
                        foreach (var add in requestImages.Where(x => x.movieImageID == 0 && x.image != null))
                        {
                            var newUrl = await _cloudinaryService.UploadImageAsync(add.image!);
                            await _movieImageRepository.AddMovieImageAsync(new MovieImage
                            {
                                movieID = movie.movieID,
                                ImageUrl = newUrl
                            }, cancellationToken);
                        }
                    }

                    // Commit 1 lần
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return true;
                }, ct: ct);

                // Xóa poster cũ (nếu có upload poster mới)
                if (request.image is not null && !string.IsNullOrWhiteSpace(oldPoster))
                {
                    await _cloudinaryService.DeleteImageAsync(oldPoster);
                }

                _logger.LogInformation("Successfully updated movie with ID: {MovieID}", request.movieID);
                return ResponseConst.Success("Successfully updated the movie", movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating movie with ID: {MovieID}", request.movieID);
                return ResponseConst.Error<Movies>(500, "Error occurred while updating the movie");
            }
        }

        public async Task<ResponseDto<bool>> DeleteMovie(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting movie with ID: {MovieID}", movieID);
            try
            {
                var movie = await _movieRepository.GetByIdAsync(movieID, ct);
                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID: {MovieID} not found", movieID);
                    return ResponseConst.Error<bool>(404, "Movie not found");
                }
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    await _movieRepository.RemoveAsync(movieID);
                    return true;
                }, ct: ct);
                _logger.LogInformation("Successfully deleted movie with ID: {MovieID}", movieID);
                return ResponseConst.Success("Successfully deleted the movie", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting movie with ID: {MovieID}", movieID);
                return ResponseConst.Error<bool>(500, "Error occurred while deleting the movie");
            }
        }
        public async Task<ResponseDto<Movies>> GetMovieByID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving movie with ID: {MovieID}", movieID);
            try
            {
                var movie = await _movieRepository.GetByIdAsync(movieID, ct);
                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID: {MovieID} not found", movieID);
                    return ResponseConst.Error<Movies>(404, "Movie not found");
                }
                _logger.LogInformation("Successfully retrieved movie with ID: {MovieID}", movieID);
                return ResponseConst.Success("Successfully retrieved the movie", movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving movie with ID: {MovieID}", movieID);
                return ResponseConst.Error<Movies>(500, "Error occurred while retrieving the movie");
            }
        }
        public async Task<ResponseDto<List<Movies>>> GetAllMovies(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all movies");
            try
            {
                var movies = await _movieRepository.GetAllMovieAsync(ct);
                _logger.LogInformation("Successfully retrieved all movies");
                return ResponseConst.Success("Successfully retrieved all movies", movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all movies");
                return ResponseConst.Error<List<Movies>>(500, "Error occurred while retrieving all movies");
            }
        }
    }
}
