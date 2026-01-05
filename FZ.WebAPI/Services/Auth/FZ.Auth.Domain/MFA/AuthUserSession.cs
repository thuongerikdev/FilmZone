using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FZ.Auth.Domain.MFA
{
    [Table(nameof(AuthUserSession), Schema = Constant.Database.DbSchema.Auth)]
    public class AuthUserSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int sessionID { get; set; }

        [ForeignKey(nameof(user))]
        public int userID { get; set; }

        public string deviceId { get; set; }
        public string ip { get; set; }
        public string userAgent { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime lastSeenAt { get; set; }
        public bool isRevoked { get; set; }

        public virtual AuthUser user { get; set; }
        public virtual ICollection<AuthRefreshToken> refreshTokens { get; set; } = new List<AuthRefreshToken>();
    }
}
