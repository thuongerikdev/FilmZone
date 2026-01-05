using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Interactions
{
    [Table(nameof(EpisodeWatchProgress), Schema = Constant.Database.DbSchema.Movie)]
    public class EpisodeWatchProgress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int episodeWatchProgressID { get; set; }

        public int userID { get; set; }       // scalar, no FK
        public int episodeID { get; set; }
        public int? episodeSourceID { get; set; }

        public int positionSeconds { get; set; }
        public int? durationSeconds { get; set; }
        public DateTime? lastWatchedAt { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(episodeID))]
        public virtual Catalog.Episode episode { get; set; }


        [ForeignKey(nameof(episodeSourceID))]
        public virtual Media.EpisodeSource? episodeSource { get; set; }
    }
}
