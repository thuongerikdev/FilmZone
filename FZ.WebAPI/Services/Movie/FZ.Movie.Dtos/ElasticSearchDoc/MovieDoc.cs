using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.ElasticSearchDoc
{
    public sealed class MovieDoc
    {
        public string Id { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? OriginalTitle { get; set; }
        public string? Description { get; set; }
        public string MovieType { get; set; } = default!; // movie | series
        public string Image { get; set; } = default!;
        public string Status { get; set; } = default!;     // ongoing | completed | coming_soon
        public DateTime? ReleaseDate { get; set; }
        public int? DurationSeconds { get; set; }
        public int? TotalSeasons { get; set; }
        public int? TotalEpisodes { get; set; }

        public int RegionId { get; set; }
        public string? RegionCode { get; set; }
        public string? RegionName { get; set; }

        public int? Year { get; set; }
        public string? Rated { get; set; }
        public double? Popularity { get; set; }

        public List<TagMini> Tags { get; set; } = new();
        public List<CastMini> Cast { get; set; } = new();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public sealed class TagMini
        {
            public int TagId { get; set; }
            public string TagName { get; set; } = default!;
            public string? Slug { get; set; } // nếu bạn muốn sinh slug từ tagName
        }

        public sealed class CastMini
        {
            public int PersonId { get; set; }
            public string FullName { get; set; } = default!;
            public string Role { get; set; } = default!; // cast|director|writer...
            public string? CharacterName { get; set; }
            public int? CreditOrder { get; set; }
        }
    }

}
