using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Media
{
    [Table(nameof(MovieSource), Schema = Constant.Database.DbSchema.Movie)]
    public class MovieSource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movieSourceID { get; set; }

        public int movieID { get; set; }

        [Required, MaxLength(32)]
        public string sourceName { get; set; }     // YouTube | Archive | Vimeo | Proxy | R2...

        [Required, MaxLength(16)]
        public string sourceType { get; set; }     // youtube | archive | vimeo | proxy | file

        [Required]
        public string sourceUrl { get; set; }      // embed/direct or /stream

        [MaxLength(256)]
        public string? sourceID { get; set; }      // VIDEO_ID / identifier

        [MaxLength(16)]
        public string? quality { get; set; }       // 1080p...

        [MaxLength(8)]
        public string? language { get; set; }      // vi | en...

        public bool isVipOnly { get; set; }
        public bool isActive { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }
    }
}
