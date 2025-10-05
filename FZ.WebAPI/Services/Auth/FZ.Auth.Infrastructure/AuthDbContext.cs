using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.MFA;
using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        public DbSet<Plan> plans { get; set; }
        public DbSet<Price> prices { get; set; }
        public DbSet<UserSubscription> userSubscriptions { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<Invoice> invoices { get; set; }
        public DbSet<Payment> payments { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ====== AUTH CORE ======
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

            // Tránh multiple cascade paths qua RefreshToken
            modelBuilder.Entity<AuthUser>()
                .HasMany(u => u.refreshTokens)
                .WithOne(rt => rt.user)
                .HasForeignKey(rt => rt.userID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuthUserSession>()
                .HasMany(s => s.refreshTokens)
                .WithOne(rt => rt.session)
                .HasForeignKey(rt => rt.sessionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuthRole>()
                .HasMany(r => r.userRoles)
                .WithOne(ur => ur.role)
                .HasForeignKey(ur => ur.roleID);

            modelBuilder.Entity<AuthRole>()
                .HasMany(r => r.rolePermissions)
                .WithOne(rp => rp.role)
                .HasForeignKey(rp => rp.roleID);

            modelBuilder.Entity<AuthUserRole>(b =>
            {
                b.ToTable("AuthUserRole", "auth");

                // ✅ Dùng composite PK chuẩn cho bảng liên kết M-N:
                b.HasKey(x => new { x.userID, x.roleID });

                // (tuỳ chọn) Nếu bạn vẫn muốn có cột Id riêng thì:
                // b.HasKey(x => x.Id);
                // b.HasIndex(x => new { x.userID, x.roleID }).IsUnique(); // bắt uniqueness

                b.Property(x => x.assignedAt)
                 .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<AuthPermission>()
                .HasMany(p => p.rolePermissions)
                .WithOne(rp => rp.permission)
                .HasForeignKey(rp => rp.permissionID);

            // ====== BILLING ======

            // Plan
            modelBuilder.Entity<Plan>(e =>
            {
                e.HasKey(x => x.planID);
                e.HasIndex(x => x.code).IsUnique();
                e.Property(x => x.name).HasMaxLength(128);

                // Plan 1 - n Price (inverse)
                e.HasMany(x => x.prices)
                 .WithOne(p => p.plan)
                 .HasForeignKey(p => p.planID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Price
            modelBuilder.Entity<Price>(e =>
            {
                e.HasKey(x => x.priceID);
                e.HasIndex(x => new { x.planID, x.currency, x.intervalUnit, x.intervalCount }).IsUnique();
                e.Property(x => x.amount).HasColumnType("decimal(18,2)");
                e.Property(x => x.intervalUnit).HasMaxLength(16);
            });

            // UserSubscription
            modelBuilder.Entity<UserSubscription>(e =>
            {
                e.HasKey(x => x.subscriptionID);

                e.HasOne(x => x.user)
                 .WithMany(u => u.subscriptions)
                 .HasForeignKey(x => x.userID)
                 .OnDelete(DeleteBehavior.Restrict);          // Cắt một nhánh cascade

                e.HasOne(x => x.plan)
                 .WithMany()
                 .HasForeignKey(x => x.planID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.price)
                 .WithMany()
                 .HasForeignKey(x => x.priceID)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(x => new { x.userID, x.planID, x.status });
                e.HasIndex(x => x.currentPeriodEnd);
            });

            // Order
            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(x => x.orderID);

                e.Property(x => x.amount).HasColumnType("decimal(18,2)");

                e.HasOne(x => x.user)
                 .WithMany(u => u.orders)
                 .HasForeignKey(x => x.userID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Order n - 1 Plan (inverse Plan.orders)
                e.HasOne(x => x.plan)
                 .WithMany(p => p.orders)
                 .HasForeignKey(x => x.planID)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

                // Order n - 1 Price (inverse Price.orders)
                e.HasOne(x => x.price)
                 .WithMany(p => p.orders)
                 .HasForeignKey(x => x.priceID)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.provider, x.providerSessionId }).IsUnique();
            });

            // Invoice
            modelBuilder.Entity<Invoice>(e =>
            {
                e.HasKey(x => x.invoiceID);

                // KHÔNG cascade từ User sang Invoice để tránh multiple paths
                e.HasOne(x => x.user)
                 .WithMany(u => u.invoices)
                 .HasForeignKey(x => x.userID)
                 .OnDelete(DeleteBehavior.NoAction);

                // Cho phép null khi Order/Sub bị xóa theo user
                e.HasOne(x => x.subscription)
                 .WithMany(s => s.invoices)
                 .HasForeignKey(x => x.subscriptionID)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.order)
                 .WithMany(o => o.invoices)
                 .HasForeignKey(x => x.orderID)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(x => new { x.userID, x.issuedAt });
            });

            // Payment
            modelBuilder.Entity<Payment>(e =>
            {
                e.HasKey(x => x.paymentID);

                e.HasOne(x => x.invoice)
                 .WithMany(i => i.payments)
                 .HasForeignKey(x => x.invoiceID)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.provider, x.providerPaymentId }).IsUnique(false);
            });


            var utcDateTimeConverter = new ValueConverter<DateTime, DateTime>(
              v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // -> DB
              v => DateTime.SpecifyKind(v, DateTimeKind.Utc)             // <- DB
          );

            // Nullable DateTime?
            var utcNullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v == null
                        ? (DateTime?)null
                        : (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()),
                v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            );

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in entity.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                        prop.SetValueConverter(utcDateTimeConverter);

                    if (prop.ClrType == typeof(DateTime?))
                        prop.SetValueConverter(utcNullableDateTimeConverter);
                }
            }
            // ======== end UTC converters =========


            // ====== Seed ======
            modelBuilder.Entity<Plan>().HasData(new Plan
            {
                planID = 1,
                code = "VIP",
                name = "Gói VIP",
                description = "Quyền lợi VIP (không quảng cáo, chất lượng cao...)",
                isActive = true
            });

            modelBuilder.Entity<Price>().HasData(
                new Price { priceID = 101, planID = 1, currency = "VND", amount = 99000m, intervalUnit = "month", intervalCount = 1, isActive = true },
                new Price { priceID = 102, planID = 1, currency = "VND", amount = 249000m, intervalUnit = "month", intervalCount = 3, isActive = true },
                new Price { priceID = 103, planID = 1, currency = "VND", amount = 459000m, intervalUnit = "month", intervalCount = 6, isActive = true }
            );

            // ====== AUTH CORE ======
            modelBuilder.Entity<AuthRole>(e =>
            {
                e.HasKey(r => r.roleID);
                e.Property(r => r.roleName).HasMaxLength(100).IsRequired();
                e.Property(r => r.roleDescription).HasMaxLength(255);

                // Khuyến nghị: tên vai trò là duy nhất
                e.HasIndex(r => r.roleName).IsUnique();

                // Seed sẵn 2 role
                e.HasData(
                    new AuthRole
                    {
                        roleID = 10,                     // đổi nếu bạn đã dùng 10
                        roleName = "customer",
                        roleDescription = "Khách hàng tiêu chuẩn",
                        isDefault = true                 // role mặc định khi tạo user mới
                    },
                    new AuthRole
                    {
                        roleID = 11,                     // đổi nếu bạn đã dùng 11
                        roleName = "customer-vip",
                        roleDescription = "Khách hàng VIP (đồng bộ với gói VIP)",
                        isDefault = false
                    }
                );
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
