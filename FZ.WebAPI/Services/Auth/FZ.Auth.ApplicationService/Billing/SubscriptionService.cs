using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.Role;
using FZ.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
            // 1) Lấy price & validate thuộc plan VIP
            var price = await _db.prices
                .Include(p => p.plan)
                .FirstOrDefaultAsync(p => p.priceID == priceId && p.isActive, ct)
                ?? throw new InvalidOperationException("Price không tồn tại");

            if (!string.Equals(price.plan.code, "VIP", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Price không thuộc Plan VIP");

            // 2) Tìm sub VIP còn hiệu lực
            var now = DateTime.UtcNow;
            var activeStatuses = new[] { "active", "trialing", "grace" };

            var sub = await _db.userSubscriptions
                .FirstOrDefaultAsync(s =>
                    s.userID == userId &&
                    s.planID == price.planID &&
                    activeStatuses.Contains(s.status),
                    ct);

            var months = price.intervalCount <= 0 ? 1 : price.intervalCount;

            if (sub == null)
            {
                // Tạo mới
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
                // Gia hạn: cộng từ max(now, currentPeriodEnd)
                var baseDate = sub.currentPeriodEnd > now ? sub.currentPeriodEnd : now;
                sub.currentPeriodEnd = baseDate.AddMonths(months);
                sub.priceID = price.priceID;
                sub.autoRenew = sub.autoRenew || autoRenew; // giữ true nếu đã bật
                _db.userSubscriptions.Update(sub);
            }

            // 3) Cấp role (idempotent)
            await GrantRoleIfMissingAsync(userId, "customer-vip", ct);
            await GrantRoleIfMissingAsync(userId, "customer", ct); // nếu muốn

            await _db.SaveChangesAsync(ct);
        }

        public async Task ExpireIfDueAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // Lấy các sub tới hạn (KHÔNG loại cancelAtPeriodEnd)
            var dueSubs = await _db.userSubscriptions
                .Where(s =>
                    (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
                    s.currentPeriodEnd <= now)
                .ToListAsync(ct);

            if (dueSubs.Count == 0) return;

            // Đánh dấu expired
            foreach (var s in dueSubs)
                s.status = "expired";

            await _db.SaveChangesAsync(ct); // lưu trước để truy vấn "still VIP" nhìn thấy trạng thái mới

            // Chỉ revoke role nếu user KHÔNG còn VIP nào khác đang hiệu lực
            var vipPlanId = await _db.plans
                .Where(p => p.code == "VIP")
                .Select(p => p.planID)
                .FirstAsync(ct);

            var affectedUsers = dueSubs.Select(s => s.userID).Distinct().ToList();

            foreach (var userId in affectedUsers)
            {
                var stillVip = await _db.userSubscriptions.AnyAsync(s =>
                        s.userID == userId &&
                        s.planID == vipPlanId &&
                        (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
                        s.currentPeriodEnd > now,
                        ct);

                if (!stillVip)
                    await RevokeRoleIfPresentAsync(userId, "customer-vip", ct);
            }

            await _db.SaveChangesAsync(ct);
        }

        // Helpers
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

        private async Task RevokeRoleIfPresentAsync(int userId, string roleName, CancellationToken ct)
        {
            var role = await _db.authRoles.FirstOrDefaultAsync(r => r.roleName == roleName, ct);
            if (role == null) return;

            var link = await _db.authUserRoles
                .FirstOrDefaultAsync(x => x.userID == userId && x.roleID == role.roleID, ct);

            if (link != null)
                _db.authUserRoles.Remove(link);
        }
    }
}
