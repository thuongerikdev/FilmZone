﻿using FZ.Auth.Domain.User;
using FZ.Auth.Domain.MFA;             // ⬅️ thêm namespace để dùng AuthUserSession
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FZ.Auth.Domain.Token
{
    [Table(nameof(AuthRefreshToken), Schema = Constant.Database.DbSchema.Auth)]
    public class AuthRefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int refreshTokenID { get; set; }

        [ForeignKey(nameof(user))]
        public int userID { get; set; }

        // ⬅️ NEW: khóa ngoại tới phiên (thiết bị)
        [ForeignKey(nameof(session))]
        public int sessionID { get; set; }

        public string Token { get; set; } = default!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;

        public virtual AuthUser user { get; set; }
        public virtual AuthUserSession session { get; set; }  // ⬅️ NEW navigation
    }
}
