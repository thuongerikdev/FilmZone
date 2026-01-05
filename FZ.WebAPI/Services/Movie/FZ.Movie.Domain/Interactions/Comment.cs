using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Interactions
{
    [Table(nameof(Comment), Schema = Constant.Database.DbSchema.Movie)]
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int commentID { get; set; }

        public int movieID { get; set; }
        public int userID { get; set; }       // scalar, no FK

        public int? parentID { get; set; }

        [Required]
        public string content { get; set; }

        public bool isEdited { get; set; }
        public int likeCount { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(movieID))]
        public virtual Catalog.Movies movie { get; set; }

        [ForeignKey(nameof(parentID))]
        public virtual Comment? parent { get; set; }

        [InverseProperty(nameof(parent))]
        public virtual ICollection<Comment> replies { get; set; }
    }
}
