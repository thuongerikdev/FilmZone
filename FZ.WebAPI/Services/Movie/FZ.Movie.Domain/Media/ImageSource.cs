using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Media
{
    [Table(nameof(ImageSource), Schema = Constant.Database.DbSchema.Movie)]
    public class ImageSource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int imageSourceID { get; set; }
        public string imageSourceName { get; set; }
        public string imageSourcetype { get; set; } // enum ImageSourceType
        public string source { get; set; }   // enum ImageSourceType
        public string status { get; set; }   // active | inactive
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
