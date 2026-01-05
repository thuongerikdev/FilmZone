using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Request
{
    public class CreateMoiveTagRequest
    {
        public int movieID { get; set; }
        public int tagID { get; set; }
    }
    public class UpdateMoiveTagRequest : CreateMoiveTagRequest
    {
        public int movieTagID { get; set; }
    }

    public class  CreateTagRequest
    {
        public string tagName { get; set; }
        public string? tagDescription { get; set; }
    }
    public class UpdateTagRequest : CreateTagRequest
    {
        public int tagID { get; set; }
    }

}
