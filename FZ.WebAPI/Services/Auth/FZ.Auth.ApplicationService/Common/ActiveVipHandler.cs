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
        // Admin thì pass luôn
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var idClaim =
        context.User.FindFirst("userId") ??
        context.User.FindFirst(ClaimTypes.NameIdentifier) ??
        context.User.FindFirst("sub");

        if (idClaim == null || !int.TryParse(idClaim.Value, out var userId)) return;




        var now = DateTime.UtcNow;
        var vipPlanId = await _db.plans
            .Where(p => p.code == "VIP")
            .Select(p => p.planID)
            .FirstOrDefaultAsync();

        if (vipPlanId == 0) return;

        var ok = await _db.userSubscriptions.AnyAsync(s =>
            s.userID == userId &&
            s.planID == vipPlanId &&
            (s.status == "active" || s.status == "trialing" || s.status == "grace") &&
            s.currentPeriodEnd > now);

        if (ok) context.Succeed(requirement);
    }
}
