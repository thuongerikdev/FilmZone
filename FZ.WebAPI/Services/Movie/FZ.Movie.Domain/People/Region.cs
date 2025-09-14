using FZ.Movie.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Domain.People
{
    [Table(nameof(Region), Schema = Constant.Database.DbSchema.Movie)]
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int regionID { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public virtual ICollection<Person> People { get; set; }
        public virtual  ICollection<Movies> Movies { get; set; }
    }
}
