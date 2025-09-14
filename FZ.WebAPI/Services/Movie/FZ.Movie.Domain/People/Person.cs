using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.People
{
    [Table(nameof(Person), Schema = Constant.Database.DbSchema.Movie)]
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int personID { get; set; }

        [Required, MaxLength(200)]
        public string fullName { get; set; }

        [MaxLength(255)]
        public string? knownFor { get; set; }
        public int regionID { get; set; }

        public string? biography { get; set; }
        [MaxLength(512)]
        public string? avatar { get; set; }
        public DateTime? birthDate { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        public virtual ICollection<MoviePerson> credits { get; set; }
        public virtual Region? region { get; set; }
        }
}
