using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.MFA;
using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.Token;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FZ.Auth.Domain.User
{
    [Table(nameof(AuthUser), Schema = Constant.Database.DbSchema.Auth)]
    public class AuthUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userID { get; set; }

        [Required, MaxLength(100)]
        public string userName { get; set; }

        [Required, MaxLength(255)]
        public string email { get; set; }

        [MaxLength(32)]
        public string? phoneNumber { get; set; }

        [Required]
        public string passwordHash { get; set; }
        public string? googleSub { get; set; }

        public bool isEmailVerified { get; set; }
        //public bool isPhoneVerified { get; set; }
        [MaxLength(16)]
        public string status { get; set; }
        public int tokenVersion { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        // Navigation
        public virtual AuthProfile profile { get; set; }
        public virtual ICollection<AuthUserRole> userRoles { get; set; }
        public virtual ICollection<AuthRefreshToken> refreshTokens { get; set; }
        public virtual AuthMfaSecret mfaSecret { get; set; }
        public virtual ICollection<AuthAuditLog> auditLogs { get; set; }
        public virtual ICollection<AuthUserSession> sessions { get; set; }
        public virtual ICollection<AuthEmailVerification> emailVerifications { get; set; }
        public virtual ICollection<AuthPasswordReset> passwordResets { get; set; }
        public virtual ICollection<FZ.Auth.Domain.Billing.UserSubscription> subscriptions { get; set; }
        public virtual ICollection<Order> orders { get; set; }
        public virtual ICollection<Invoice> invoices { get; set; }

    }


   
}
