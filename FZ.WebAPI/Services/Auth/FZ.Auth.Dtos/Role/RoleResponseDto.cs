﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.Role
{
    public class RoleResponse
    {
        public string roleName { get; set; }
        public string roleDescription { get; set; }
        public bool isDefault { get; set; } = false; // Vai trò mặc định khi tạo user mới
    }
    //public class GetUserRolesResponse
    //{
    //    public int userID { get; set; }
    //    public string FullName { get; set; }
    //    public List<string> roles { get; set; }
    //}
}
