using FZ.Movie.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Media
{
    [Table(nameof(MovieImage), Schema = Constant.Database.DbSchema.Movie)]

    public class MovieImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // ✅ tự tăng
        public int movieImageID { get; set; }

        public int movieID { get; set; }                       // FK -> Movies.movieID

        public string ImageUrl { get; set; } = default!;

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(movieID))]
        public virtual Movies Movie { get; set; } = default!;  // (nên để số ít cho rõ nghĩa)
    }
}
