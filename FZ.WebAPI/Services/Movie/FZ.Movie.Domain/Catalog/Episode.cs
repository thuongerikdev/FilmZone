using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Catalog
{
    [Table(nameof(Episode), Schema = Constant.Database.DbSchema.Movie)]
    public class Episode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int episodeID { get; set; }

        public int movieID { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }

        [Required, MaxLength(255)]
        public string title { get; set; }

        [MaxLength(512)]
        public string? synopsis { get; set; }

        public string? description { get; set; }
        public int? durationSeconds { get; set; }
        public DateTime? releaseDate { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Movies movie { get; set; }
        public virtual ICollection<Media.EpisodeSource> sources { get; set; }
    }
}
