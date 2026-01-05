using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Taxonomy
{
    [Table(nameof(Tag), Schema = Constant.Database.DbSchema.Movie)]
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int tagID { get; set; }

        [Required, MaxLength(120)]
        public string tagName { get; set; }

        [MaxLength(512)]
        public string? tagDescription { get; set; }

        public DateTime createAt { get; set; }
        public DateTime updateAt { get; set; }

        // Navigation
        public virtual ICollection<MovieTag> movieTags { get; set; }
    }
}
