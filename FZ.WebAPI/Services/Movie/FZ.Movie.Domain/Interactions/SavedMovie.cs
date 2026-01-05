using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Interactions
{
    [Table(nameof(SavedMovie), Schema = Constant.Database.DbSchema.Movie)]
    public class SavedMovie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int savedMovieID { get; set; }

        public int movieID { get; set; }
        public int userID { get; set; }       // scalar, no FK

        public DateTime createdAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }
    }
}
