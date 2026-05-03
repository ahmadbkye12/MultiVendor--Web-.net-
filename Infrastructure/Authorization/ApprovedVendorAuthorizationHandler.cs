using Application.Authorization;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

public sealed class ApprovedVendorAuthorizationHandler(ApplicationDbContext db)
    : AuthorizationHandler<ApprovedVendorRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApprovedVendorRequirement requirement)
    {
        if (!context.User.IsInRole(AuthRoles.Vendor))
            return;

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return;

        var approved = await db.Vendors.AsNoTracking().AnyAsync(
            v => v.OwnerUserId == userId && v.IsApproved && !v.IsDeleted);

        if (approved)
            context.Succeed(requirement);
    }
}
