using Application.Vendor;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorReviewService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorReviewService
{
    public async Task<IReadOnlyList<VendorReviewListItemDto>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        take = Math.Clamp(take, 1, 200);
        skip = Math.Max(0, skip);

        var storeIds = scope.StoreIds.ToHashSet();
        if (storeIds.Count == 0)
            return [];

        var query =
            from r in db.Reviews.AsNoTracking()
            join p in db.Products.AsNoTracking() on r.ProductId equals p.Id
            where !r.IsDeleted && !p.IsDeleted && storeIds.Contains(p.VendorStoreId)
            orderby r.CreatedAtUtc descending
            select new VendorReviewListItemDto(r.Id, p.Id, p.Name, r.CustomerUserId, r.Rating, r.Comment, r.CreatedAtUtc);

        return await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }
}
