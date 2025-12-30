using FZ.Auth.Infrastructure;
using FZ.Auth.Domain.User; // Để dùng AuthUserRole
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FZ.Auth.ApplicationService.Common
{
    public class SubscriptionExpiryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionExpiryWorker> _logger;

        public SubscriptionExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpiryWorker> logger)
        {
            _scopeFactory = scopeFactory; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                    var now = DateTime.UtcNow;

                    // 1. Tìm các subscription đã hết hạn
                    // Join sẵn bảng Plan để lấy RoleID luôn cho tiện
                    var dueSubs = await db.userSubscriptions
                        .Include(s => s.plan) // <--- JOIN Plan
                        .Where(s =>
                            (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
                            s.currentPeriodEnd <= now)
                        .ToListAsync(stoppingToken);

                    if (dueSubs.Count > 0)
                    {
                        foreach (var sub in dueSubs)
                        {
                            // A. Đổi trạng thái sub thành expired
                            sub.status = "expired";

                            // B. XỬ LÝ ROLE DYNAMIC
                            // Lấy RoleID gắn với Plan của Sub này
                            var roleIdToRemove = sub.plan?.roleID;

                            // Nếu Plan này có gắn role (roleId > 0)
                            if (roleIdToRemove.HasValue && roleIdToRemove.Value > 0)
                            {
                                var rId = roleIdToRemove.Value;
                                var uId = sub.userID;

                                // C. Kiểm tra an toàn (Idempotent check):
                                // User này còn Sub nào KHÁC (đang active) mà cũng cung cấp RoleID này không?
                                // (Ví dụ: Mua 2 gói VIP 1 tháng liên tiếp, gói 1 hết hạn nhưng gói 2 vẫn còn -> Không được cắt Role)
                                var stillHasSameRole = await db.userSubscriptions
                                    .Include(x => x.plan)
                                    .AnyAsync(x =>
                                        x.userID == uId &&
                                        x.subscriptionID != sub.subscriptionID && // Sub khác
                                        x.plan.roleID == rId &&                   // Cùng Role
                                        (x.status == "active" || x.status == "trialing" || x.status == "grace") &&
                                        x.currentPeriodEnd > now,
                                        stoppingToken);

                                if (!stillHasSameRole)
                                {
                                    // D. Nếu không còn gói nào cung cấp role này -> Xóa Role
                                    var userRole = await db.authUserRoles
                                        .FirstOrDefaultAsync(ur => ur.userID == uId && ur.roleID == rId, stoppingToken);

                                    if (userRole != null)
                                    {
                                        db.authUserRoles.Remove(userRole);
                                        _logger.LogInformation("Revoked RoleID {RoleId} from User {UserId} because Plan {PlanCode} expired.", rId, uId, sub.plan?.code);
                                    }
                                }
                            }
                        }

                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expire subscription job failed");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}