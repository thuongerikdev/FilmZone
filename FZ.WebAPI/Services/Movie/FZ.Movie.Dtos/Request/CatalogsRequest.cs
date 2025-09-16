using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Request
{

    public class CreateMovieImage
    {
        public IFormFile image { get; set; }
    }
    public class UpdateMovieImage
    {
        public int movieImageID { get; set; }
        public int movieID { get; set; }
        public IFormFile image { get; set; }
    }
    public class CreateMoviePerson
    {
        public int personID { get; set; }
        public string role { get; set; }               // cast | director | writer ...
        public string? characterName { get; set; }
        public int? creditOrder { get; set; }
    }


    public  class CreateMoviesRequest
    {
        public string slug { get; set; }
        public string title { get; set; }
        public IFormFile image { get; set; }
        public string? originalTitle { get; set; }
        public string? description { get; set; }
        public string movieType { get; set; }     // movie | series
        public string status { get; set; }        // ongoing | completed | coming_soon
        public DateTime? releaseDate { get; set; }
        public int? durationSeconds { get; set; }
        public int? totalSeasons { get; set; }
        public int? totalEpisodes { get; set; }

        public int? year { get; set; }
        public string? rated { get; set; }
        public int regionID { get; set; }
        public double? popularity { get; set; }
        
        public List <int>? tagIDs { get; set; } 
        public List<CreateMoviePerson>? person { get; set; }
        public List<CreateMovieImage> movieImages { get; set; }

    }






    public class UpdateMoviesRequest 
    {
        public string slug { get; set; }
        public string title { get; set; }
        public IFormFile image { get; set; }
        public string? originalTitle { get; set; }
        public string? description { get; set; }
        public string movieType { get; set; }     // movie | series
        public string status { get; set; }        // ongoing | completed | coming_soon
        public DateTime? releaseDate { get; set; }
        public int? durationSeconds { get; set; }
        public int? totalSeasons { get; set; }
        public int? totalEpisodes { get; set; }

        public int? year { get; set; }
        public string? rated { get; set; }
        public int regionID { get; set; }
        public double? popularity { get; set; }

        public List<int>? tagIDs { get; set; }
        public List<CreateMoviePerson>? person { get; set; }

        public int movieID { get; set; }
        public List<UpdateMovieImage> MovieImage { get; set; }

    }

    public class CreateEpisodeRequest
    {
        public int movieID { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
        public string title { get; set; }

        public string? synopsis { get; set; }

        public string? description { get; set; }
        public int? durationSeconds { get; set; }
        public DateTime? releaseDate { get; set; }
    }
    public class UpdateEpisodeRequest : CreateEpisodeRequest
    { 
        public int episodeID { get; set; }
    }
}
