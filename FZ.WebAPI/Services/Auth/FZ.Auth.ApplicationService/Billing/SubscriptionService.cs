using FZ.Auth.Domain.Billing;
using FZ.Auth.Domain.Role;
using FZ.Auth.Infrastructure;
using FZ.Constant;
using Microsoft.EntityFrameworkCore;
using Npgsql; // <-- thêm
using System;
using System.Linq;

namespace FZ.Auth.ApplicationService.Billing
{
    public interface ISubscriptionService
    {
        Task ActivateVipAsync(int userId, int priceId, bool autoRenew, CancellationToken ct);
        Task ExpireIfDueAsync(CancellationToken ct);
        Task<ResponseDto<bool>> CancelSubscriptionAsync(int userId, CancellationToken ct);
        Task <ResponseDto<List<UserSubscription>>> GetAllSubscription();
        Task <ResponseDto<UserSubscription>> GetSubscriptionByID(int subscriptionID, CancellationToken ct);
        Task<ResponseDto<List<UserSubscription>>> GetSubscriptionByUserID(int userID, CancellationToken ct);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly AuthDbContext _db;
        public SubscriptionService(AuthDbContext db) { _db = db; }

        public async Task ActivateVipAsync(int userId, int priceId, bool autoRenew, CancellationToken ct)
        {
            // 1) Lấy price & validate (Include Plan để lấy RoleID)
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
                // sub.status = "active"; // Nếu đang grace/trial thì active lại
                _db.userSubscriptions.Update(sub);
            }

            // 3) Lưu Sub trước
            await _db.SaveChangesAsync(ct);

            // 4) Gán Role DYNAMIC dựa trên Plan
            // Nếu Plan có gắn RoleID (> 0) thì cấp quyền đó cho user
            if (price.plan.roleID > 0)
            {
                await GrantRoleByIdIfMissingAsync(userId, price.plan.roleID, ct);
            }

            // 5) Save lần 2 – bắt lỗi trùng lặp Role (Idempotent)
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsAuthUserRoleDuplicate(ex))
            {
                // Ignore lỗi trùng khóa chính (User đã có role này rồi)
            }
        }

        public async Task ExpireIfDueAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // 1. Tìm các subscription cần hết hạn (kèm thông tin Plan)
            var expiredSubs = await _db.userSubscriptions
                .Include(s => s.plan) // <--- Quan trọng: Join bảng Plan để biết Role nào cần gỡ
                .Where(s => (s.status == "active" || s.status == "trialing" || s.status == "grace")
                         && s.currentPeriodEnd <= now)
                .ToListAsync(ct);

            if (!expiredSubs.Any()) return;

            foreach (var sub in expiredSubs)
            {
                // a. Đổi trạng thái
                sub.status = "expired";
                sub.autoRenew = false;

                // b. Xử lý gỡ Role DYNAMIC
                var roleIdToRemove = sub.plan?.roleID ?? 0;

                // Chỉ xử lý nếu Plan đó thực sự có Role
                if (roleIdToRemove > 0)
                {
                    // c. Kiểm tra an toàn: User còn gói nào KHÁC cũng cung cấp RoleID này không?
                    // (Ví dụ: Mua gói VIP 1 tháng và VIP 1 năm chồng nhau)
                    var hasOtherActiveSubWithSameRole = await _db.userSubscriptions
                        .Include(x => x.plan)
                        .AnyAsync(x => x.userID == sub.userID
                                    && x.subscriptionID != sub.subscriptionID // Không phải gói đang xét
                                    && x.plan.roleID == roleIdToRemove        // Cùng Role
                                    && (x.status == "active" || x.status == "trialing" || x.status == "grace")
                                    && x.currentPeriodEnd > now, ct);

                    // Nếu không còn gói nào bảo kê Role này -> Gỡ Role
                    if (!hasOtherActiveSubWithSameRole)
                    {
                        var userRole = await _db.authUserRoles
                            .FirstOrDefaultAsync(ur => ur.userID == sub.userID && ur.roleID == roleIdToRemove, ct);

                        if (userRole != null)
                        {
                            _db.authUserRoles.Remove(userRole);
                        }
                    }
                }
            }

            await _db.SaveChangesAsync(ct);
        }
        public async Task<ResponseDto<bool>> CancelSubscriptionAsync(int userId, CancellationToken ct)
        {
            var sub = await _db.userSubscriptions
                .FirstOrDefaultAsync(s => s.userID == userId
                                       && (s.status == "active" || s.status == "trialing"), ct);

            if (sub == null)
            {
                return ResponseConst.Error<bool>(404, "Bạn không có gói đăng ký nào đang hoạt động.");
            }

            if (sub.autoRenew == false)
            {
                return ResponseConst.Error<bool>(400, "Gói đăng ký này đã được huỷ gia hạn trước đó.");
            }

            sub.autoRenew = false;
            _db.userSubscriptions.Update(sub);
            await _db.SaveChangesAsync(ct);

            return ResponseConst.Success("Đã huỷ gia hạn gói thành công.", true);
        }

        private async Task GrantRoleByIdIfMissingAsync(int userId, int roleId, CancellationToken ct)
        {
            // Kiểm tra xem User đã có RoleID này chưa
            var has = await _db.authUserRoles
                .AnyAsync(ur => ur.userID == userId && ur.roleID == roleId, ct);

            if (!has)
            {
                _db.authUserRoles.Add(new AuthUserRole
                {
                    userID = userId,
                    roleID = roleId,
                    assignedAt = DateTime.UtcNow
                });
            }
        }

        // Helper: nhận diện lỗi trùng key ở bảng AuthUserRole
        private static bool IsAuthUserRoleDuplicate(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                // Kiểm tra tên bảng hoặc constraint name
                if (pg.TableName?.Contains("AuthUserRole", StringComparison.OrdinalIgnoreCase) == true ||
                    pg.ConstraintName?.Contains("AuthUserRole", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
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
        public async Task<ResponseDto<List<UserSubscription>>> GetSubscriptionByUserID(int userID, CancellationToken ct)
        {
            var subscriptions = await _db.userSubscriptions
                         .Where(s => s.userID == userID) // 1. Bỏ ct ở đây
                         .ToListAsync(ct);
            if (subscriptions == null)
            {
                return ResponseConst.Error<List<UserSubscription>>(404, "Subscription không tồn tại");
            }
            return ResponseConst.Success("Lấy subscription thành công", subscriptions);
        }
    }
}
