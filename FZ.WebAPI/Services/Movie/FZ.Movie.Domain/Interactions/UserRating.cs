using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Interactions
{
    [Table(nameof(UserRating), Schema = Constant.Database.DbSchema.Movie)]
    public class UserRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userRatingID { get; set; }     // surrogate PK,
        // composite PK (userID, movieID) cấu hình trong OnModelCreating
        public int userID { get; set; }       // scalar, no FK
        public int movieID { get; set; }

        [Range(1, 5)]
        public int stars { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }
    }

}
