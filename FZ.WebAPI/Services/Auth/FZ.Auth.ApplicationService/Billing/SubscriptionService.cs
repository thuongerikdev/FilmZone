using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.Role;
using FZ.Auth.Infrastructure;
using FZ.Constant;
using Microsoft.EntityFrameworkCore;
using Npgsql; // <-- thêm
using System;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface ISubscriptionService
    {
        Task ActivateVipAsync(int userId, int priceId, bool autoRenew, CancellationToken ct);
        Task ExpireIfDueAsync(CancellationToken ct);
        Task<ResponseDto<bool>> CancelSubscriptionAsync(int userId, CancellationToken ct);
        Task <ResponseDto<List<UserSubscription>>> GetAllSubscription();
        Task <ResponseDto<UserSubscription>> GetSubscriptionByID(int subscriptionID, CancellationToken ct);
        Task <ResponseDto<UserSubscription>> GetSubscriptionByUserID(int userID, CancellationToken ct);
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

        public async Task ExpireIfDueAsync(CancellationToken ct)
        {
            // 1. Lấy Role VIP để lát nữa gỡ khỏi user
            var vipRole = await _db.authRoles
                .FirstOrDefaultAsync(r => r.roleName == "customer-vip", ct);

            if (vipRole == null) return; // Chưa seed role thì không cần làm gì

            // 2. Tìm các subscription đang active nhưng đã quá hạn (currentPeriodEnd <= UtcNow)
            // Lưu ý: Chỉ xử lý những cái không autoRenew hoặc logic gia hạn thất bại (ở đây làm đơn giản: cứ quá hạn là cắt)
            var expiredSubs = await _db.userSubscriptions
                .Where(s => (s.status == "active" || s.status == "trialing")
                         && s.currentPeriodEnd <= DateTime.UtcNow)
                .ToListAsync(ct);

            if (!expiredSubs.Any()) return;

            // 3. Xử lý từng sub
            foreach (var sub in expiredSubs)
            {
                // a. Đổi trạng thái sang canceled
                sub.status = "canceled";
                sub.autoRenew = false;

                // b. Tìm UserRole tương ứng để xoá
                var userRole = await _db.authUserRoles
                    .FirstOrDefaultAsync(ur => ur.userID == sub.userID && ur.roleID == vipRole.roleID, ct);

                if (userRole != null)
                {
                    _db.authUserRoles.Remove(userRole);
                }
            }

            // 4. Lưu thay đổi
            await _db.SaveChangesAsync(ct);
        }

        public async Task<ResponseDto<bool>> CancelSubscriptionAsync(int userId, CancellationToken ct)
        {
            // 1. Tìm subscription đang chạy của user
            var sub = await _db.userSubscriptions
                .FirstOrDefaultAsync(s => s.userID == userId
                                       && (s.status == "active" || s.status == "trialing"), ct);

            if (sub == null)
            {
                return ResponseConst.Error<bool>(404, "Bạn không có gói đăng ký nào đang hoạt động.");
            }

            // 2. Logic Huỷ:
            // Cách 1 (Chuẩn SaaS): Tắt gia hạn tự động, khách vẫn dùng được đến hết ngày hết hạn.
            // Khi chạy ExpireIfDueAsync, hệ thống sẽ tự cắt role khi đến ngày.
            if (sub.autoRenew == false)
            {
                return ResponseConst.Error<bool>(400, "Gói đăng ký này đã được huỷ gia hạn trước đó.");
            }

            sub.autoRenew = false;
            _db.userSubscriptions.Update(sub);

            /* Cách 2 (Huỷ ngay lập tức - nếu nghiệp vụ yêu cầu):
               sub.status = "canceled";
               sub.currentPeriodEnd = DateTime.UtcNow; // Kết thúc ngay
               // Gọi logic gỡ Role ngay lập tức ở đây hoặc để ExpireIfDueAsync quét sau
            */

            await _db.SaveChangesAsync(ct);

            return ResponseConst.Success("Đã huỷ gia hạn gói thành công. Bạn vẫn có thể sử dụng quyền VIP đến hết chu kỳ hiện tại.", true);
        }

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
        public async Task<ResponseDto<List<UserSubscription>>> GetAllSubscription()
        {
            var subscriptions = await _db.userSubscriptions.AsNoTracking().ToListAsync();
            return ResponseConst.Success("Lấy danh sách subscription thành công", subscriptions);
        }
        public async Task<ResponseDto<UserSubscription>> GetSubscriptionByID(int subscriptionID, CancellationToken ct)
        {
            var subscription = await _db.userSubscriptions.AsNoTracking()
                                        .FirstOrDefaultAsync(s => s.subscriptionID == subscriptionID, ct);
            if (subscription == null)
            {
                return ResponseConst.Error<UserSubscription>(404, "Subscription không tồn tại");
            }
            return ResponseConst.Success("Lấy subscription thành công", subscription);
        }
        public async Task<ResponseDto<UserSubscription>> GetSubscriptionByUserID(int userID, CancellationToken ct)
        {
            var subscription = await _db.userSubscriptions.AsNoTracking()
                                        .FirstOrDefaultAsync(s => s.userID == userID, ct);
            if (subscription == null)
            {
                return ResponseConst.Error<UserSubscription>(404, "Subscription không tồn tại");
            }
            return ResponseConst.Success("Lấy subscription thành công", subscription);
        }
    }
}
