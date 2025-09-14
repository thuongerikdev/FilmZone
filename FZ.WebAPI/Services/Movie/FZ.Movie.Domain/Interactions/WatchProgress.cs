using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Interactions
{

    [Table(nameof(WatchProgress), Schema = Constant.Database.DbSchema.Movie)]
    public class WatchProgress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int watchProgressID { get; set; }

        public int userID { get; set; }       // scalar, no FK
        public int movieID { get; set; }
        public int? sourceID { get; set; }    // nullable

        public int positionSeconds { get; set; }
        public int? durationSeconds { get; set; }
        public DateTime? lastWatchedAt { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }

        [ForeignKey(nameof(sourceID))]
        public virtual Media.MovieSource? source { get; set; }
    }
}
