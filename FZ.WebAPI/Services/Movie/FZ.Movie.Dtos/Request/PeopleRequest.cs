using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Movie.Dtos.Request
{
    public class CreateMoviePersonRequest
    {
        public int movieID { get; set; }
        public int personID { get; set; }
        public string role { get; set; }               // cast | director | writer ...
        public string? characterName { get; set; }
        public int? creditOrder { get; set; }
    }
    public class UpdateMoviePersonRequest : CreateMoviePersonRequest
    {
        public int moviePersonID { get; set; }
    }

    public class  CreatePersonRequest 
    {
        public string fullName { get; set; }
        public string? knownFor { get; set; }
        public string? biography { get; set; }
        public int regionID { get; set; }
        public IFormFile? avatar { get; set; }
        public DateTime? birthDate { get; set; }
    }
    public class UpdatePersonRequest : CreatePersonRequest
    {
        public int personID { get; set; }
    }
    public class CreateRegionRequest 
    {
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }
    public class UpdateRegionRequest : CreateRegionRequest
    {
        public int regionID { get; set; }
    }

}
