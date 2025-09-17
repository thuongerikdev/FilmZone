using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Respone
{
    public class GetAllMovieMainScreenResponse
    {
        public int movieID { get; set; }

        public string slug { get; set; }

        public string title { get; set; }
        public string? originalTitle { get; set; }

        public string? description { get; set; }

        public string movieType { get; set; }     // movie | series

        public string image { get; set; }       // url hình đại diện
    }

    public class WatchNowMovieResponse
    {
        public int movieID { get; set; }

        public string slug { get; set; }
        public string title { get; set; }
        public string? originalTitle { get; set; }
        public string? description { get; set; }
        public string movieType { get; set; }     // movie | series
        public string image { get; set; }       // url hình đại diện
        public string status { get; set; }        // ongoing | completed | coming_soon
        public DateTime? releaseDate { get; set; }
        public int? durationSeconds { get; set; }
        public int? totalSeasons { get; set; }
        public int? totalEpisodes { get; set; }
        public int? year { get; set; }
        public string? rated { get; set; }
        public double? popularity { get; set; }
        public RegionNowPlayingMovieResponse? region{ get; set; }

        public List<ListTagNowPlayingMovieResponse>? tags { get; set; }
        public List<ListMovieSourceNowPlayingResponse>? sources { get; set; }
        public List<ListActorsNowPlayingMovieResponse>? actors { get; set; }
        public List<ListImagesNowPlayingMovieResponse>? images { get; set; }
      
    }
    public class ListTagNowPlayingMovieResponse
    {
        public int tagID { get; set; }
        public string tagName { get; set; }
        public string? tagDescription { get; set; }
    }
    public class ListMovieSourceNowPlayingResponse
    {
        public int movieSourceID { get; set; }
        public int movieID { get; set; }
        public string sourceName { get; set; }
    }
    public class ListActorsNowPlayingMovieResponse
    {
        public string fullName { get; set; }
        public string? avatar { get; set; }
        public int personID { get; set; }
        public string role { get; set; }               // cast | director | writer ...
        public string? characterName { get; set; }
        public int? creditOrder { get; set; }
    }
    public class ListImagesNowPlayingMovieResponse
    {
        public int movieImageID { get; set; }
        public string imageUrl { get; set; }
    }
    public class RegionNowPlayingMovieResponse
    {
        public int regionID { get; set; }
        public string regionName { get; set; }
    }


}
