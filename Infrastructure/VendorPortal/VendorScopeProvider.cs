using System.Security.Claims;
using Application.Vendor;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorScopeProvider(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    : IVendorScopeProvider
{
    public async Task<VendorScope?> GetScopeAsync(CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        var vendor = await db.Vendors.AsNoTracking()
            .FirstOrDefaultAsync(
                v => v.OwnerUserId == userId && !v.IsDeleted && v.IsApproved,
                cancellationToken);

        if (vendor is null)
            return null;

        var stores = await db.VendorStores.AsNoTracking()
            .Where(s => s.VendorId == vendor.Id && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        return new VendorScope(vendor.Id, stores, vendor.DefaultCommissionPercent);
    }
}
