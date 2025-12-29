using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.Role
{
    public class CreatePermissionRequestDto
    {
        public string permissionName { get; set; }
        public string permissionDescription { get; set; }
        public string code { get; set; }
        public string scope { get; set; }
    }
    public class UpdatePermissionRequestDto : CreatePermissionRequestDto
    {
        public int permissionID { get; set; }
    }

    public class RolePermissionRequestDto
    {
        public int roleID { get; set; }
        public List<int> permissionIDs { get; set; }
    }

}
