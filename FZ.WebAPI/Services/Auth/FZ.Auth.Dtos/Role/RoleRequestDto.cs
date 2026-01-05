using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.Role
{
   public class AddRoleRequest
   {
        public string roleName { get; set; }
        public string roleDescription { get; set; }
        public bool isDefault { get; set; } = false; // Vai trò mặc định khi tạo user mới
   }
    public class UpdateRoleRequest : AddRoleRequest
    {
        public int roleID { get; set; }
    
    }
}
