using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Request
{
    //CreateCommentRequest
    public class CreateCommentRequest
    {
        public int movieID { get; set; }
        public int userID { get; set; }       // scalar, no FK

        public int? parentID { get; set; }
        public string content { get; set; }

        public int likeCount { get; set; }
    }
    public class UpdateCommentRequest : CreateCommentRequest
    {
        public int commentID { get; set; }
    }
    //CreateEpisodeWatchProgressRequest

    public class CreateEpisodeWatchProgressRequest
    {
        public int userID { get; set; }       // scalar, no FK
        public int episodeID { get; set; }
        public int? episodeSourceID { get; set; }
        public int positionSeconds { get; set; }
        public int? durationSeconds { get; set; }
    }
    public class UpdateEpisodeWatchProgressRequest : CreateEpisodeWatchProgressRequest
    {
        public int episodeWatchProgressID { get; set; }
    }

    //CreateWatchProgressRequest
    public class CreateSavedMovieRequest
    {
        public int userID { get; set; }       // scalar, no FK
        public int movieID { get; set; }
    }
    public class UpdateSavedMovieRequest : CreateSavedMovieRequest
    {
        public int savedMovieID { get; set; }
    }

    //CreateWatchProgressRequest
    public class CreateUserRatingRequest
    {
        public int userID { get; set; }       // scalar, no FK
        public int movieID { get; set; }
        public int rating { get; set; }    // 0.5 - 5.0
    }
    public class UpdateUserRatingRequest : CreateUserRatingRequest
    {
        public int userRatingID { get; set; }
    }

    //CreateWatchProgressRequest
    public class CreateWatchProgressRequest
    {

        public int userID { get; set; }       // scalar, no FK
        public int movieID { get; set; }
        public int? sourceID { get; set; }    // nullable
        public int positionSeconds { get; set; }
        public int? durationSeconds { get; set; }
    }
    public class UpdateWatchProgressRequest : CreateWatchProgressRequest
    {
        public int watchProgressID { get; set; }
    }
}
