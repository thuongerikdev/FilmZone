using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.Media
{
    [Table(nameof(MovieSubTitle), Schema = Constant.Database.DbSchema.Movie)]
    public class MovieSubTitle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movieSubTitleID { get; set; }
        public int movieSourceID { get; set; }
        public string subTitleName { get; set; }     // English | Vietnamese | Chinese...
        public string linkSubTitle { get; set; }      // link subtitle file
        public string language { get; set; }      // vi | en...
        public bool isActive { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        // Navigation
        public virtual MovieSource movieSource { get; set; }

    }
}
