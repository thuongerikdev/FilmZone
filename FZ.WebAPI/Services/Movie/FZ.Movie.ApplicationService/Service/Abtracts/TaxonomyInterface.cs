using FZ.Constant;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Taxonomy;
using FZ.Movie.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
    public interface IMovieTagService
    {
        Task<ResponseDto<MovieTag>> CreateMovieTag(CreateMoiveTagRequest request  , CancellationToken ct);
        Task<ResponseDto<MovieTag>> UpdateMovieTag(UpdateMoiveTagRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteMovieTag(int movieTagID, CancellationToken ct);
        Task<ResponseDto<MovieTag>> GetMovieTagByID(int movieTagID, CancellationToken ct);
        Task<ResponseDto<List<MovieTag>>> GetAllMovieTags(CancellationToken ct);
        Task <ResponseDto<List<Movies>>> GetMoviesByTagIDs (List<int>  tagIDs, CancellationToken ct);
        Task<ResponseDto<List<Tag>>> GetTagByMovieID(int movieID, CancellationToken ct);

    }
    public  interface ITagService
    {
        Task<ResponseDto<Tag>> CreateTag(CreateTagRequest request, CancellationToken ct);
        Task<ResponseDto<Tag>> UpdateTag(UpdateTagRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteTag(int tagID, CancellationToken ct);
        Task<ResponseDto<Tag>> GetTagByID(int tagID, CancellationToken ct);
       
        Task<ResponseDto<List<Tag>>> GetAllTags(CancellationToken ct);
    }
}
