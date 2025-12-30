using FZ.Auth.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ActiveVipRequirement : IAuthorizationRequirement { }

public class ActiveVipHandler : AuthorizationHandler<ActiveVipRequirement>
{
    private readonly AuthDbContext _db;
    public ActiveVipHandler(AuthDbContext db) { _db = db; }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ActiveVipRequirement requirement)
    {
        // 1. Admin luôn pass
        if (context.User.IsInRole("admin")) // Lưu ý: role name thường viết thường trong DB của bạn
        {
            context.Succeed(requirement);
            return;
        }

        // 2. Lấy UserId
        var idClaim = context.User.FindFirst("userId") ??
                      context.User.FindFirst(ClaimTypes.NameIdentifier) ??
                      context.User.FindFirst("sub");

        if (idClaim == null || !int.TryParse(idClaim.Value, out var userId)) return;

        var now = DateTime.UtcNow;

        // 3. LOGIC MỚI: Dynamic
        // Không tìm code="VIP" nữa.
        // Kiểm tra user có BẤT KỲ gói đăng ký nào còn hạn hay không.
        // Vì bất kỳ gói nào trong bảng UserSubscription đều là gói trả phí (hoặc trial).
        var hasActiveSub = await _db.userSubscriptions.AnyAsync(s =>
            s.userID == userId &&
            (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
            s.currentPeriodEnd > now);

        if (hasActiveSub)
        {
            context.Succeed(requirement);
        }
    }
}
