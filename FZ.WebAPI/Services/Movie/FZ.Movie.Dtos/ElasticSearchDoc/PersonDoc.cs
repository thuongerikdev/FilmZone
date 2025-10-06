using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.ElasticSearchDoc
{
    public sealed class PersonDoc
    {
        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? KnownFor { get; set; }

        public int RegionId { get; set; }
        public string? RegionCode { get; set; }
        public string? RegionName { get; set; }

        public string? Biography { get; set; }
        public string? Avatar { get; set; }
        public DateTime? BirthDate { get; set; }

        // Filmography rút gọn
        public List<CreditMini> Credits { get; set; } = new();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public sealed class CreditMini
        {
            public int MovieId { get; set; }
            public string Title { get; set; } = default!;
            public int? Year { get; set; }
            public string Role { get; set; } = default!;
            public string? CharacterName { get; set; }
            public int? CreditOrder { get; set; }
        }
    }

}
