using FZ.Constant;
using FZ.Movie.ApplicationService.Common;
using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Media;
using FZ.Movie.Domain.People;
using FZ.Movie.Domain.Taxonomy;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Dtos.Respone;
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
        public async Task<ResponseDto<MovieCreatedDto>> CreateMovie(CreateMoviesRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Creating a new movie with slug: {Slug}", request.slug);

            try
            {
                // 1) Kiểm tra trùng slug
                var existingMovie = await _movieRepository.GetBySlugAsync(request.slug, ct);
                if (existingMovie != null)
                {
                    _logger.LogWarning("Movie with slug: {Slug} already exists", request.slug);
                    return ResponseConst.Error<MovieCreatedDto>(400, "Movie with the same slug already exists");
                }

                // 2) Upload poster (bắt buộc theo BE hiện tại)
                var posterUrl = await _cloudinaryService.UploadImageAsync(request.image);

                // 3) Chuẩn bị list input an toàn null
                var tagIdsInput = (request.tagIDs ?? new List<int>()).Distinct().ToList();
                var peopleInput = (request.person ?? new List<CreateMoviePerson>()).ToList();
                var imagesInput = (request.movieImages ?? new List<CreateMovieImage>()).ToList();

                // 4) Thực thi trong transaction
                await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
                {
                    // 4.1) Tạo movie
                    var newMovie = new Movies
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
                        image = posterUrl,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                    };

                    await _movieRepository.AddAsync(newMovie, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken); // cần để có movieID

                    // 4.2) Thêm tags
                    foreach (var tagID in tagIdsInput)
                    {
                        await _movieTagRepository.AddAsync(new MovieTag
                        {
                            movieID = newMovie.movieID,
                            tagID = tagID
                        }, cancellationToken);
                    }

                    // 4.3) Thêm people
                    foreach (var p in peopleInput)
                    {
                        await _moviePersonRepository.AddAsync(new MoviePerson
                        {
                            movieID = newMovie.movieID,
                            personID = p.personID,
                            role = p.role,
                            characterName = p.characterName,
                            creditOrder = p.creditOrder
                        }, cancellationToken);
                    }

                    // 4.4) Upload & thêm images
                    var createdImageDtos = new List<MovieImageDto>(imagesInput.Count);
                    foreach (var img in imagesInput)
                    {
                        // Bỏ qua phần tử null hoặc không có file
                        if (img?.image == null) continue;

                        var url = await _cloudinaryService.UploadImageAsync(img.image);
                        var movieImage = new MovieImage
                        {
                            movieID = newMovie.movieID,
                            ImageUrl = url,
                            createdAt = DateTime.UtcNow
                        };
                        await _movieImageRepository.AddMovieImageAsync(movieImage, cancellationToken);

                        // Lưu ý: movieImageID sẽ có sau SaveChanges
                    }

                    // 4.5) Lưu 1 lần cho batch (tags, people, images)
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // 4.6) Load lại IDs ảnh vừa tạo (nếu cần ID), hoặc tạo DTO từ request/URL đã upload
                    // Nếu repo có method lấy images theo movieID:
                    var savedImages = await _movieImageRepository.GetByMovieID(newMovie.movieID, cancellationToken);
                    createdImageDtos = savedImages
                        .Select(mi => new MovieImageDto(mi.movieImageID, mi.ImageUrl))
                        .ToList();

                    // 4.7) Tạo DTO kết quả, KHÔNG trả entity để tránh cycle
                    var resultDto = new MovieCreatedDto(
                        movieID: newMovie.movieID,
                        slug: newMovie.slug,
                        title: newMovie.title,
                        description: newMovie.description,
                        image: newMovie.image,
                        movieType: newMovie.movieType,
                        status: newMovie.status,
                        releaseDate: newMovie.releaseDate,
                        durationSeconds: newMovie.durationSeconds,
                        totalSeasons: newMovie.totalSeasons,
                        totalEpisodes: newMovie.totalEpisodes,
                        year: newMovie.year,
                        rated: newMovie.rated,
                        popularity: newMovie.popularity ?? 0,
                        regionID: newMovie.regionID,
                        tagIDs: tagIdsInput,
                        people: peopleInput.Select(p => new MoviePersonDto(p.personID, p.role, p.characterName, p.creditOrder)).ToList(),
                        images: createdImageDtos
                    );

                    // Trả ra trong transaction scope
                    // (tùy UoW ExecuteInTransactionAsync cho phép return object nào)
                    // Ở đây trả bool rồi tạo Response ở ngoài
                    // => Ta gán vào 1 biến bên ngoài qua closure:
                    created = resultDto;

                    return true;
                }, ct: ct);

                _logger.LogInformation("Successfully created a new movie with slug: {Slug}", request.slug);
                return ResponseConst.Success("Successfully created a new movie", created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new movie with slug: {Slug}", request.slug);
                return ResponseConst.Error<MovieCreatedDto>(500, "Error occurred while creating a new movie");
            }
        }

        // biến tạm để lấy dto ra khỏi closure (đặt trong class hoặc method scope)
        private MovieCreatedDto? created;

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

        public async Task<ResponseDto<List<GetAllMovieMainScreenResponse>>> GetAllMoviesMainScreen(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all movies for main screen");
            try
            {
                var movies = await _movieRepository.GetAllMovieMainScreenAsync(ct);
                _logger.LogInformation("Successfully retrieved all movies for main screen");
                return ResponseConst.Success("Successfully retrieved all movies for main screen", movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all movies for main screen");
                return ResponseConst.Error<List<GetAllMovieMainScreenResponse>>(500, "Error occurred while retrieving all movies for main screen");
            }
        }
        public async Task<ResponseDto<List<GetAllMovieMainScreenResponse>>> GetAllMoviesNewReleaseMainScreen(CancellationToken ct)
        {
            _logger.LogInformation("Retrieving all new release movies for main screen");
            try
            {
                var movies = await _movieRepository.GetAllMovieNewReleaseMainScreenAsync(ct);
                _logger.LogInformation("Successfully retrieved all new release movies for main screen");
                return ResponseConst.Success("Successfully retrieved all new release movies for main screen", movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all new release movies for main screen");
                return ResponseConst.Error<List<GetAllMovieMainScreenResponse>>(500, "Error occurred while retrieving all new release movies for main screen");
            }
        }
        public async Task<ResponseDto<WatchNowMovieResponse>> GetWatchNowMovieByID(int movieID, CancellationToken ct)
        {
            _logger.LogInformation("Retrieving watch now movie with ID: {MovieID}", movieID);
            try
            {
                var movie = await _movieRepository.WatchNowMovieResponse(movieID, ct);
                if (movie == null)
                {
                    _logger.LogWarning("Watch now movie with ID: {MovieID} not found", movieID);
                    return ResponseConst.Error<WatchNowMovieResponse>(404, "Movie not found");
                }
                _logger.LogInformation("Successfully retrieved watch now movie with ID: {MovieID}", movieID);
                return ResponseConst.Success("Successfully retrieved the watch now movie", movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving watch now movie with ID: {MovieID}", movieID);
                return ResponseConst.Error<WatchNowMovieResponse>(500, "Error occurred while retrieving the watch now movie");
            }
        }
    }
}
