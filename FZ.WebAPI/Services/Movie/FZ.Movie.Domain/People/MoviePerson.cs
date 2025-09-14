using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.People
{
    [Table(nameof(MoviePerson), Schema = Constant.Database.DbSchema.Movie)]
    public class MoviePerson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int moviePersonID { get; set; }     // surrogate PK,
        // Composite key cấu hình trong OnModelCreating
        public int movieID { get; set; }
        public int personID { get; set; }

        [MaxLength(24)]
        public string role { get; set; }               // cast | director | writer ...
        [MaxLength(255)]
        public string? characterName { get; set; }
        public int? creditOrder { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }

        [ForeignKey(nameof(personID))]
        public virtual Person person { get; set; }
    }
}
