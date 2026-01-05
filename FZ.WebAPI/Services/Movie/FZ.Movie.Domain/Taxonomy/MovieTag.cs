using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Taxonomy
{
    [Table(nameof(MovieTag), Schema = Constant.Database.DbSchema.Movie)]
    public class MovieTag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movieTagID { get; set; }

        public int movieID { get; set; }
        public int tagID { get; set; }
        public DateTime createAt { get; set; }
        public DateTime? updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }

        [ForeignKey(nameof(tagID))]
        public virtual Tag tag { get; set; }

        public DateTime? createdAt { get; set; }
    }
}
