using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos
{
    public class UploadJob
    {
        [Key] public string jobId { get; set; } = Guid.NewGuid().ToString("N");
        public string sourceType { get; set; }    // vimeo | youtube | archive
        public string scope { get; set; }         // movie | episode
        public int targetId { get; set; }         // movieID or episodeID
        public string fileName { get; set; }
        public long fileSize { get; set; }
        public string status { get; set; }        // Queued|Uploading|Processing|Completed|Failed
        public int progress { get; set; }         // 0..100 (upload progress)
        public string? vendorVideoUri { get; set; }   // /videos/{id} (Vimeo) | videoId (YT) | identifier (IA)
        public string? vendorUploadUrl { get; set; }  // Vimeo tus upload_link (optional)
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string? error { get; set; }
    }


    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
        public string Scope { get; set; } = "movie"; // "movie" | "episode"
        public int TargetId { get; set; }
        public string? Quality { get; set; }
        public string? Language { get; set; }
        public bool IsVipOnly { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UploadLinkRequest
    {
        public string LinkUrl { get; set; } // URL file video PUBLIC
        public string Scope { get; set; } = "movie";
        public int TargetId { get; set; }
        public string? Quality { get; set; }
        public string? Language { get; set; }
        public bool IsVipOnly { get; set; }
        public bool IsActive { get; set; } = true;
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




}
