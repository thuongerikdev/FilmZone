using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Request
{
    public class CreateEpisodeSourceRequest
    {
        public int episodeID { get; set; }
        public string sourceName { get; set; }
        public string sourceType { get; set; }
        public string sourceUrl { get; set; }
        public string? sourceId { get; set; }
        public string? quality { get; set; }
        public string? language { get; set; }
        public bool isVipOnly { get; set; }
        public bool isActive { get; set; }
    }
    public class UpdateEpisodeSourceRequest : CreateEpisodeSourceRequest
    {
        public int episodeSourceID { get; set; }
    }

    public class CreateMovieSourceRequest
    {
        public int movieID { get; set; }
        public string sourceName { get; set; }     // YouTube | Archive | Vimeo | Proxy | R2...
        public string sourceType { get; set; }     // youtube | archive | vimeo | proxy | file
        public string sourceUrl { get; set; }      // embed/direct or /stream
        public string? sourceId { get; set; }      // VIDEO_ID / identifier
        public string? quality { get; set; }       // 1080p...
        public string? language { get; set; }      // vi | en...
        public string? rawSubTitle { get; set; }  // raw data of subtitle if any
        public bool isVipOnly { get; set; }
        public bool isActive { get; set; }
    }
    public class UpdateMovieSourceRequest : CreateMovieSourceRequest
    {
        public int sourceID { get; set; }
    }

    public class CreateImageSourceRequest
    {
        public string imageSourceName { get; set; }
        public string imageSourcetype { get; set; } // enum ImageSourceType
        public IFormFile source { get; set; }   // enum ImageSourceType
        public string status { get; set; }   // active | inactive
    }
    public class UpdateImageSourceRequest : CreateImageSourceRequest
    {
        public int imageSourceID { get; set; }
    }

    public class CreateMovieImageRequest
    {
        public int movieID { get; set; }
        public IFormFile imageUrl { get; set; }   // enum ImageSourceType
    }



    public class UpsertEpisodeSourceFromVendorRequest
    {
        public int EpisodeId { get; set; }
        public string SourceName { get; set; } = "Vimeo";      // Vimeo | YouTube | Archive...
        public string SourceType { get; set; } = "vimeo";      // vimeo|youtube|archive|...
        public string SourceUrl { get; set; }                  // player/embed url
        public string SourceId { get; set; }                   // vendor video id (bắt buộc với vimeo/yt)
        public string? Quality { get; set; } = "1080p";
        public string? Language { get; set; } = "vi";
        public bool IsVipOnly { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpsertMovieSourceFromVendorRequest
    {
        public int MovieId { get; set; }
        public string SourceName { get; set; } = "Vimeo";
        public string SourceType { get; set; } = "vimeo";
        public string SourceUrl { get; set; }
        public string SourceId { get; set; }                   // string!
        public string? Quality { get; set; } = "1080p";
        public string? Language { get; set; } = "vi";
        public bool IsVipOnly { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateMovieSubTitleRequest
    {
        public int movieSourceID { get; set; }
        public string language { get; set; }      // vi | en...
  
        public string subTitleName { get; set; }     // English | Vietnamese | Chinese...
        public string linkSubTitle { get; set; }      // link subtitle file
        public bool isActive { get; set; } = true;


    }
    public class UpdateMovieSubTitleRequest : CreateMovieSubTitleRequest
    {
        public int movieSubTitleID { get; set; }
    }

    public class CreateEpisodeSubTitleRequest
    {
        public int episodeSourceID { get; set; }
        public string language { get; set; }      // vi | en...
        public string subTitleName { get; set; }     // English | Vietnamese | Chinese...
        public string linkSubTitle { get; set; }      // link subtitle file
        public bool isActive { get; set; } = true;
    }
    public class UpdateEpisodeSubTitleRequest : CreateEpisodeSubTitleRequest
    {
        public int episodeSubTitleID { get; set; }
    }

   

    //public class CreateSubTitleByFile
    //{

    //}
}
