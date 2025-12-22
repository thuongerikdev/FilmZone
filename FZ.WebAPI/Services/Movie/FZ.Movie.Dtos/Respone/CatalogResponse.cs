using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

    public class SegmentDto
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; } = string.Empty;
    }

    public class TranslateSourceRawRequest
    {
        // Thông tin để tìm kiếm trong DB
        public int sourceID { get; set; }
        public string type { get; set; } = "movie"; // "movie" hoặc "episode"

        // Thông tin cấu hình External API
        public string externalApiUrl { get; set; }
        public string apiToken { get; set; }

        // Ngôn ngữ muốn dịch sang
        public string targetLanguage { get; set; } = "vi";
    }
    public class RawSegmentDto
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
    }

    // DTO Mới: Dành cho Webhook callback
    public class AutoGenerateSubTitleRequest
    {
        // Các trường Input từ Client
        public int sourceID { get; set; } // ID của Movie hoặc Episode
        public IFormFile videoFile { get; set; }
        public string externalApiUrl { get; set; } = "http://localhost:8000";
        public string? apiToken { get; set; }

        // Trường này Service tự gán (Client không cần gửi)
        public string type { get; set; } // "movie" hoặc "episode"
    }

    public class TranscribeCallbackRequest
    {
        // Các trường API Python trả về
        [JsonPropertyName("type")]
        public string type { get; set; } // "movie" hoặc "episode"

        [JsonPropertyName("source_id")]
        public int sourceID { get; set; }

        public string srt { get; set; }
        public string language { get; set; }

        [JsonPropertyName("raw_segments")]
        public List<SegmentItemDto> RawSegments { get; set; }
    }

    public class SegmentItemDto
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
    }


    // DTO để hứng response từ API Python (Task ID)
    public class TranscribeTaskResponse
    {
        public string task_id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }


}
