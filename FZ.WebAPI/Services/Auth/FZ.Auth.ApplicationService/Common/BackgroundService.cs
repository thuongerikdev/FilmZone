using FZ.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // chạy mỗi 5 phút (tùy chỉnh)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                    var now = DateTime.UtcNow;
                    var dueSubs = await db.userSubscriptions
                        .Where(s =>
                            (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
                            s.currentPeriodEnd <= now)
                        .Select(s => new { s.subscriptionID, s.userID, s.planID })
                        .ToListAsync(stoppingToken);

                    if (dueSubs.Count > 0)
                    {
                        var subIds = dueSubs.Select(x => x.subscriptionID).ToList();
                        var subs = await db.userSubscriptions
                            .Where(s => subIds.Contains(s.subscriptionID))
                            .ToListAsync(stoppingToken);

                        foreach (var s in subs)
                        {
                            s.status = "expired";
                        }

                        // Thu hồi role customer-vip nếu user không còn VIP nào khác còn hạn
                        var vipPlanId = await db.plans.Where(p => p.code == "VIP").Select(p => p.planID).FirstAsync(stoppingToken);

                        var affectedUsers = dueSubs.Select(x => x.userID).Distinct().ToList();
                        foreach (var userId in affectedUsers)
                        {
                            var stillVip = await db.userSubscriptions
                                .AnyAsync(x =>
                                    x.userID == userId &&
                                    x.planID == vipPlanId &&
                                    (x.status == "active" || x.status == "trialing" || x.status == "grace") &&
                                    x.currentPeriodEnd > now,
                                    stoppingToken);

                            if (!stillVip)
                            {
                                // revoke role idempotent
                                var vipRole = await db.authRoles.FirstOrDefaultAsync(r => r.roleName == "customer-vip", stoppingToken);
                                if (vipRole != null)
                                {
                                    var link = await db.authUserRoles
                                        .FirstOrDefaultAsync(ur => ur.userID == userId && ur.roleID == vipRole.roleID, stoppingToken);
                                    if (link != null) db.authUserRoles.Remove(link);
                                }
                            }
                        }

                        await db.SaveChangesAsync(stoppingToken);

                        // (tuỳ chọn) gửi thông báo: SignalR / email
                        // await hub.Clients.User(userIdStr).SendAsync("subscription.expired", ...);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expire VIP job failed");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

}
