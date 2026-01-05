using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Media
{
    [Table(nameof(EpisodeSource), Schema = Constant.Database.DbSchema.Movie)]
    public class EpisodeSource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int episodeSourceID { get; set; }

        public int episodeID { get; set; }

        [Required, MaxLength(32)]
        public string sourceName { get; set; }

        [Required, MaxLength(16)]
        public string sourceType { get; set; }

        [Required]
        public string sourceUrl { get; set; }

        [MaxLength(256)]
        public string? sourceID { get; set; }

        [MaxLength(16)]
        public string? quality { get; set; }

        [MaxLength(8)]
        public string? language { get; set; }

        public bool isVipOnly { get; set; }
        public bool isActive { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(episodeID))]
        public virtual Catalog.Episode episode { get; set; }
    }
}
