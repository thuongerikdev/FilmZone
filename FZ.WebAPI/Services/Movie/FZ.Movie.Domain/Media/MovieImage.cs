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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int movieImageID { get; set; }    // PK, same as movieID
        public int movieID { get; set; }          // FK to Movies.movieID
        public string ImageUrl { get; set; }
        public DateTime createdAt { get; set; }

        public virtual Movies Movies { get; set; }
        }
}
