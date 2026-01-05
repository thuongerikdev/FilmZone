using FZ.Movie.Domain.Interactions;
using FZ.Movie.Domain.Media;
using FZ.Movie.Domain.People;
using FZ.Movie.Domain.Taxonomy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FZ.Movie.Domain.Catalog
{
    [Table(nameof(Movies), Schema = Constant.Database.DbSchema.Movie)]
    public class Movies
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movieID { get; set; }

        [Required, MaxLength(255)]
        public string slug { get; set; }

        [Required, MaxLength(255)]
        public string title { get; set; }

        [MaxLength(255)]
        public string? originalTitle { get; set; }

        public string? description { get; set; }

        [MaxLength(16)]
        public string movieType { get; set; }     // movie | series

        public string image { get; set; }       // url hình đại diện

        [MaxLength(32)]
        public string status { get; set; }        // ongoing | completed | coming_soon

        public DateTime? releaseDate { get; set; }
        public int? durationSeconds { get; set; }
        public int? totalSeasons { get; set; }
        public int? totalEpisodes { get; set; }
        public int regionID { get; set; }
        public int? year { get; set; }

        [MaxLength(16)]
        public string? rated { get; set; }

        public double? popularity { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        public virtual ICollection<MovieTag> movieTags { get; set; }
        public virtual ICollection<MovieSource> sources { get; set; }
        public virtual ICollection<Episode> episodes { get; set; }
        public virtual ICollection<SavedMovie> savedBy { get; set; }
        public virtual ICollection<UserRating> ratings { get; set; }
        public virtual ICollection<Comment> comments { get; set; }
        public virtual ICollection<MoviePerson> credits { get; set; }
        public virtual ICollection<WatchProgress> watchProgresses { get; set; }
        public virtual ICollection<MovieImage> movieImages { get; set; }
        public virtual Region? regions { get; set; }
        }
}
