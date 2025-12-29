using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Constant
{
    public static class PermissionConstants
    {
        public static readonly Dictionary<string, string> Permissions = new()
        {
            { "MovieGetAdmin", "Movie.get.admin" },
            
            { "MovieGetUser",  "Movie.get.user" },
            
            { "MovieGetStaff", "Movie.get.staff" },

            { "MovieCreateAdmin", "Movie.create.admin" },
            { "UserDeleteAdmin",  "User.delete.admin" }
        };
    }
}
