using FZ.Constant;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Media;
using FZ.Movie.Domain.People;
using FZ.Movie.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.ApplicationService.Service.Abtracts
{
    public interface IPersonService
    {
        Task<ResponseDto<Person>> CreatePerson(CreatePersonRequest request, CancellationToken ct);
        Task<ResponseDto<Person>> UpdatePerson(UpdatePersonRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeletePerson(int personID, CancellationToken ct);
        Task<ResponseDto<Person>> GetPersonByID(int personID, CancellationToken ct);
        Task<ResponseDto<List<Person>>> GetPeople(CancellationToken ct);


    }
    public interface IMoviePersonService
    {
        Task<ResponseDto<MoviePerson>> CreateMoviePerson(CreateMoviePersonRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteMoviePerson(int moviePersonID, CancellationToken ct);
        Task<ResponseDto<List<Person>>> GetCreditsByMovieID(int movieID, CancellationToken ct);
        Task<ResponseDto<List<Movies>>> GetMoviesByPersonID(int personID, CancellationToken ct);
        Task<ResponseDto<List<MoviePerson>>> GetMoviePersonsByPersonID(int personID, CancellationToken ct);
    }
    public interface IRegionService
    {
        Task<ResponseDto<Region>> CreateRegion(CreateRegionRequest request, CancellationToken ct);
        Task<ResponseDto<Region>> UpdateRegion(UpdateRegionRequest request, CancellationToken ct);
        Task<ResponseDto<bool>> DeleteRegion(int regionID, CancellationToken ct);
        Task<ResponseDto<Region>> GetRegionByID(int regionID, CancellationToken ct);
        Task<ResponseDto<List<Region>>> GetAllRegions(CancellationToken ct);
        Task<ResponseDto<List<Movies>>> GetMoviesByRegionIDAsync(int regionID, CancellationToken ct);
        Task<ResponseDto<List<Person>>> GetPeopleByRegionID(int regionID, CancellationToken ct);



    }
}
