using FZ.Constant;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Dtos.Request;
using FZ.Movie.Dtos.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
     public interface IMoviesService
     {
        Task<ResponseDto<MovieCreatedDto>> CreateMovie(CreateMoviesRequest request, CancellationToken ct);
            Task<ResponseDto<Movies>> UpdateMovie (UpdateMoviesRequest request , CancellationToken ct);
            Task<ResponseDto<bool>> DeleteMovie(int movieID, CancellationToken ct);
            Task<ResponseDto<Movies>> GetMovieByID (int movieID, CancellationToken ct);
            Task<ResponseDto<List<Movies>>> GetAllMovies (CancellationToken ct);
            Task<ResponseDto<List<GetAllMovieMainScreenResponse>>> GetAllMoviesMainScreen (CancellationToken ct);
            Task<ResponseDto<List<GetAllMovieMainScreenResponse>>> GetAllMoviesNewReleaseMainScreen (CancellationToken ct);
        Task<ResponseDto<WatchNowMovieResponse>> GetWatchNowMovieByID(int movieID , CancellationToken ct);

    }
    public interface IEpisodeService
    {
        Task<ResponseDto<Episode>> CreateEpisode (CreateEpisodeRequest request , CancellationToken ct);
        Task<ResponseDto<Episode>> UpdateEpisode (UpdateEpisodeRequest request , CancellationToken ct);
        Task<ResponseDto<bool>> DeleteEpisode (int episodeID, CancellationToken ct);
        Task<ResponseDto<Episode>> GetEpisodeByID (int episodeID, CancellationToken ct);
        Task<ResponseDto<List<Episode>>> GetAllEpisode (CancellationToken ct);
    }


    
}
