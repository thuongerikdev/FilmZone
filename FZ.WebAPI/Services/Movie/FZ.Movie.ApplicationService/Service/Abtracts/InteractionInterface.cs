using FZ.Constant;
using FZ.Movie.Domain.Interactions;
using FZ.Movie.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
    public interface ICommentService
    {
        Task<ResponseDto<Comment>> CreateComment (CreateCommentRequest request, CancellationToken ct);
        Task<ResponseDto<Comment>> UpdateComment (UpdateCommentRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteComment (int commentID, CancellationToken ct);
        Task<ResponseDto<Comment>> GetCommentByID (int commentID, CancellationToken ct);
        Task<ResponseDto<List<Comment>>> GetCommentsByUserID (int userID, CancellationToken ct);
        Task<ResponseDto<List<Comment>>> GetCommentsByMovieID (int movieID, CancellationToken ct);

    }
    public interface IEpisodeWatchProgressService
    {
        Task<ResponseDto<EpisodeWatchProgress>> CreateEpisodeWatchProgress (CreateEpisodeWatchProgressRequest request, CancellationToken ct);
        Task<ResponseDto<EpisodeWatchProgress>> UpdateEpisodeWatchProgress (UpdateEpisodeWatchProgressRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteEpisodeWatchProgress (int episodeWatchProgressID, CancellationToken ct);
        Task<ResponseDto<EpisodeWatchProgress>> GetEpisodeWatchProgressByID (int episodeWatchProgressID, CancellationToken ct);
        Task<ResponseDto<List<EpisodeWatchProgress>>> GetEpisodeWatchProgressByUserID (int userID, CancellationToken ct);
        Task<ResponseDto<List<EpisodeWatchProgress>>> GetEpisodeWatchProgressByEpisodeID (int episodeID, CancellationToken ct);
    }
    public interface ISavedMovieService
    {
        Task<ResponseDto<SavedMovie>> CreateSavedMovie (CreateSavedMovieRequest request, CancellationToken ct);
        Task<ResponseDto<SavedMovie>> UpdateSavedMovie (UpdateSavedMovieRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteSavedMovie (int savedMovieID, CancellationToken ct);
        Task<ResponseDto<SavedMovie>> GetSavedMovieByID (int savedMovieID, CancellationToken ct);
        Task<ResponseDto<List<SavedMovie>>> GetSavedMoviesByUserID (int userID, CancellationToken ct);
        Task<ResponseDto<List<SavedMovie>>> GetSavedMoviesByMovieID (int movieID, CancellationToken ct);
    }
    public interface IUserRatingService
    {
        Task<ResponseDto<UserRating>> CreateUserRating (CreateUserRatingRequest request, CancellationToken ct);
        Task<ResponseDto<UserRating>> UpdateUserRating (UpdateUserRatingRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteUserRating (int userRatingID, CancellationToken ct);
        Task<ResponseDto<UserRating>> GetUserRatingByID (int userRatingID, CancellationToken ct);
        Task<ResponseDto<List<UserRating>>> GetUserRatingsByUserID (int userID, CancellationToken ct);
        Task<ResponseDto<List<UserRating>>> GetUserRatingsByMovieID (int movieID, CancellationToken ct);
    }
    public interface IWatchProgressService
    {
        Task<ResponseDto<WatchProgress>> CreateWatchProgress (CreateWatchProgressRequest request, CancellationToken ct);
        Task<ResponseDto<WatchProgress>> UpdateWatchProgress (UpdateWatchProgressRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteWatchProgress (int watchProgressID, CancellationToken ct);
        Task<ResponseDto<WatchProgress>> GetWatchProgressByID (int watchProgressID, CancellationToken ct);
        Task<ResponseDto<List<WatchProgress>>> GetWatchProgressByUserID (int userID, CancellationToken ct);
        Task<ResponseDto<List<WatchProgress>>> GetWatchProgressByMovieID (int movieID, CancellationToken ct);
    }
}
