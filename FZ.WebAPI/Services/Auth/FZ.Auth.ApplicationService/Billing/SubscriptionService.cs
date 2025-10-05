using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.Role;
using FZ.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql; // <-- thêm
using System;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface ISubscriptionService
    {
        Task ActivateVipAsync(int userId, int priceId, bool autoRenew, CancellationToken ct);
        Task ExpireIfDueAsync(CancellationToken ct);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly AuthDbContext _db;
        public SubscriptionService(AuthDbContext db) { _db = db; }

        public async Task ActivateVipAsync(int userId, int priceId, bool autoRenew, CancellationToken ct)
        {
            // 1) Lấy price & validate
            var price = await _db.prices
                .Include(p => p.plan)
                .FirstOrDefaultAsync(p => p.priceID == priceId && p.isActive, ct)
                ?? throw new InvalidOperationException("Price không tồn tại");

            if (!string.Equals(price.plan.code, "VIP", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Price không thuộc Plan VIP");

            var now = DateTime.UtcNow;
            var activeStatuses = new[] { "active", "trialing", "grace" };

            // 2) Tạo mới / gia hạn subscription
            var sub = await _db.userSubscriptions
                .FirstOrDefaultAsync(s =>
                    s.userID == userId &&
                    s.planID == price.planID &&
                    activeStatuses.Contains(s.status),
                    ct);

            var months = price.intervalCount <= 0 ? 1 : price.intervalCount;

            if (sub == null)
            {
                sub = new UserSubscription
                {
                    userID = userId,
                    planID = price.planID,
                    priceID = price.priceID,
                    status = "active",
                    autoRenew = autoRenew,
                    startAt = now,
                    currentPeriodStart = now,
                    currentPeriodEnd = now.AddMonths(months)
                };
                _db.userSubscriptions.Add(sub);
            }
            else
            {
                var baseDate = sub.currentPeriodEnd > now ? sub.currentPeriodEnd : now;
                sub.currentPeriodEnd = baseDate.AddMonths(months);
                sub.priceID = price.priceID;
                sub.autoRenew = sub.autoRenew || autoRenew;
                _db.userSubscriptions.Update(sub);
            }

            // 3) Lưu riêng phần subscription trước (để không bị rollback vì lỗi role)
            await _db.SaveChangesAsync(ct);

            // 4) Gán role (giữ logic AnyAsync như cũ)
            await GrantRoleIfMissingAsync(userId, "customer-vip", ct);
            await GrantRoleIfMissingAsync(userId, "customer", ct);

            // 5) Save lần 2 – bắt 23505 nếu trùng (race)
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsAuthUserRoleDuplicate(ex))
            {
                // Một request khác đã gán role song song -> coi như thành công
            }
        }

        public async Task ExpireIfDueAsync(CancellationToken ct) { /* giữ nguyên của bạn */ }

        private async Task GrantRoleIfMissingAsync(int userId, string roleName, CancellationToken ct)
        {
            var role = await _db.authRoles.FirstOrDefaultAsync(r => r.roleName == roleName, ct)
                       ?? throw new InvalidOperationException($"Role '{roleName}' chưa seed");

            var has = await _db.authUserRoles
                .AnyAsync(ur => ur.userID == userId && ur.roleID == role.roleID, ct);

            if (!has)
            {
                _db.authUserRoles.Add(new AuthUserRole
                {
                    userID = userId,
                    roleID = role.roleID,
                    assignedAt = DateTime.UtcNow
                });
            }
        }

        // Helper: nhận diện lỗi trùng key ở bảng AuthUserRole
        private static bool IsAuthUserRoleDuplicate(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg &&
                pg.SqlState == "23505" &&
                // tuỳ environment, ConstraintName có thể là "PK_AuthUserRole" hoặc index unique khác
                (string.Equals(pg.ConstraintName, "PK_AuthUserRole", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(pg.TableName, "AuthUserRole", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
