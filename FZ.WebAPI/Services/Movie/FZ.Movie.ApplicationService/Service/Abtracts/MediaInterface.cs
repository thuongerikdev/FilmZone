using FZ.Constant;
using FZ.Movie.Domain.Media;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
    public interface IEpisodeSourceService
    {
        Task<ResponseDto<EpisodeSource>> CreateEpisodeSource(CreateEpisodeSourceRequest request, CancellationToken ct);
        Task<ResponseDto<EpisodeSource>> UpdateEpisodeSource(UpdateEpisodeSourceRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteEpisodeSource(int episodeSourceID, CancellationToken ct);
        Task<ResponseDto<EpisodeSource>> GetEpisodeSourceByID(int episodeSourceID, CancellationToken ct);
        Task<ResponseDto<List<EpisodeSource>>> GetEpisodeSourcesByEpisodeID(int episodeID, CancellationToken ct);
        Task<ResponseDto<EpisodeSource>> UpsertFromVendorAsync(UpsertEpisodeSourceFromVendorRequest request, CancellationToken ct);
    }
    public interface IMovieSourceService
    {
        Task<ResponseDto<MovieSource>> CreateMovieSource(CreateMovieSourceRequest request, CancellationToken ct);
        Task<ResponseDto<MovieSource>> UpdateMovieSource(UpdateMovieSourceRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteMovieSource(int movieSourceID, CancellationToken ct);
        Task<ResponseDto<MovieSource>> GetMovieSourceByID(int movieSourceID, CancellationToken ct);
        Task<ResponseDto<List<MovieSource>>> GetMovieSourcesByMovieID(int movieID, CancellationToken ct);
        Task<ResponseDto<MovieSource>> UpsertFromVendorAsync(UpsertMovieSourceFromVendorRequest request, CancellationToken ct);
    }
    public interface  IImageSourceService
    {
        Task<ResponseDto<ImageSource>> CreateImageSource(CreateImageSourceRequest request, CancellationToken ct);
        Task<ResponseDto<ImageSource>> UpdateImageSource(UpdateImageSourceRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteImageSource(int imageSourceID, CancellationToken ct);
        Task<ResponseDto<ImageSource>> GetImageSourceByID(int imageSourceID, CancellationToken ct);
        Task<ResponseDto<List<ImageSource>>> GetImageSourcesByTpe(string type, CancellationToken ct);
       

    }
    public interface IMovieSubTitleService
    {
        Task<ResponseDto<MovieSubTitle>> AutoGenerateSubTitleAsync(AutoGenerateSubTitleRequest autoGenerateSubTitleRequest, CancellationToken ct);



        Task<ResponseDto<MovieSubTitle>> CreateMovieSubTitle(CreateMovieSubTitleRequest request, CancellationToken ct);
        Task<ResponseDto<MovieSubTitle>> UpdateMovieSubTitle(UpdateMovieSubTitleRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteMovieSubTitle(int movieSubTitleID, CancellationToken ct);
        Task<ResponseDto<MovieSubTitle>> GetMovieSubTitleByID(int movieSubTitleID, CancellationToken ct);
        Task<ResponseDto<List<MovieSubTitle>>> GetMovieSubTitlesByMovieSourceID(int movieSourceID, CancellationToken ct);
        Task<ResponseDto<List<MovieSubTitle>>> GetAllMovieSubTitile (CancellationToken ct);
        
    }
}
