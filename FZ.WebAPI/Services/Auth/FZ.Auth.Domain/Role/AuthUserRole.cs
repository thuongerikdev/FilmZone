using FZ.Auth.Domain.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Domain.Role
{
    [Table(nameof(AuthUserRole), Schema = Constant.Database.DbSchema.Auth)]
    public class AuthUserRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int userID { get; set; }
        public int roleID { get; set; }
        public DateTime assignedAt { get; set; }

        public virtual AuthUser user { get; set; }
        public virtual AuthRole role { get; set; }
    }

}
