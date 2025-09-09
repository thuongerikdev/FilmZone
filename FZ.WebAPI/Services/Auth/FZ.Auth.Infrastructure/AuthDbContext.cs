using FZ.Auth.Domain.MFA;
using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace FZ.Auth.Infrastructure
{
    public class AuthDbContext : DbContext
    {
        public DbSet<AuthUser> authUsers { get; set; }
        public DbSet<AuthProfile> authProfiles { get; set; }
        public DbSet<AuthPermission> authPermissions { get; set; }
        public DbSet<AuthRole> authRoles { get; set; }
        public DbSet<AuthUserRole> authUserRoles { get; set; }
        public DbSet<AuthRolePermission> authRolePermissions { get; set; }
        public DbSet<AuthRefreshToken> authRefreshTokens { get; set; }
        public DbSet<AuthEmailVerification> authEmailVerifications { get; set; }
        public DbSet<AuthPasswordReset> authPasswordResets { get; set; }
        public DbSet<AuthMfaSecret> authMfaSecrets { get; set; }
        public DbSet<AuthAuditLog> authAuditLogs { get; set; }
        public DbSet<AuthUserSession> authUserSessions { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthUser>()
                .HasOne(u => u.profile)
                .WithOne(p => p.user)
                .HasForeignKey<AuthProfile>(p => p.userID);

            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.userRoles)
                .WithOne(ur => ur.user)
                .HasForeignKey(ur => ur.userID);

            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.auditLogs)
                .WithOne(al => al.user)
                .HasForeignKey(al => al.userID);

            modelBuilder.Entity<AuthUser>()
                .HasOne(u => u.mfaSecret)
                .WithOne(ms => ms.user)
                .HasForeignKey<AuthMfaSecret>(ms => ms.userID);

            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.sessions)
                .WithOne(s => s.user)
                .HasForeignKey(s => s.userID);

            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.emailVerifications)
                .WithOne(ev => ev.user)
                .HasForeignKey(ev => ev.userID);

            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.passwordResets)
                .WithOne(pr => pr.user)
                .HasForeignKey(pr => pr.userID);

            // ❌ tránh multiple cascade paths
            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.refreshTokens)
                .WithOne(rt => rt.user)
                .HasForeignKey(rt => rt.userID)
                .OnDelete(DeleteBehavior.Restrict); // 🔑 bỏ cascade ở đây

            modelBuilder.Entity<AuthUserSession>()
                .HasMany(s => s.refreshTokens)
                .WithOne(rt => rt.session)
                .HasForeignKey(rt => rt.sessionID)
                .OnDelete(DeleteBehavior.Cascade); // cascade theo session

            modelBuilder.Entity<AuthRole>()
                .HasMany(r => r.userRoles)
                .WithOne(ur => ur.role)
                .HasForeignKey(ur => ur.roleID);

            modelBuilder.Entity<AuthRole>()
                .HasMany(r => r.rolePermissions)
                .WithOne(rp => rp.role)
                .HasForeignKey(rp => rp.roleID);

            modelBuilder.Entity<AuthPermission>()
                .HasMany(p => p.rolePermissions)

                .WithOne(rp => rp.permission)
                .HasForeignKey(rp => rp.permissionID);

            base.OnModelCreating(modelBuilder);
        }
    }
}
