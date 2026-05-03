using Application.Vendor;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorEarningsService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorEarningsService
{
    public async Task<VendorEarningsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = scope.StoreIds.ToHashSet();
        if (storeIds.Count == 0)
        {
            return new VendorEarningsSummaryDto(0, 0, 0, 0);
        }

        var itemsQuery = db.OrderItems.Where(i =>
            !i.IsDeleted &&
            storeIds.Contains(i.VendorStoreId) &&
            !i.Order.IsDeleted &&
            i.Order.Status == OrderStatus.Delivered);

        var totalVendorNet = await itemsQuery.SumAsync(i => i.VendorNetAmount, cancellationToken);
        var totalCommission = await itemsQuery.SumAsync(i => i.CommissionAmount, cancellationToken);
        var totalLine = await itemsQuery.SumAsync(i => i.LineTotal, cancellationToken);

        var completedOrderCount = await itemsQuery.Select(i => i.OrderId).Distinct().CountAsync(cancellationToken);

        return new VendorEarningsSummaryDto(totalVendorNet, totalCommission, totalLine, completedOrderCount);
    }

    public async Task<IReadOnlyList<VendorEarningLineDto>> ListLinesAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        take = Math.Clamp(take, 1, 500);
        skip = Math.Max(0, skip);

        var storeIds = scope.StoreIds.ToHashSet();
        if (storeIds.Count == 0)
            return [];

        var query =
            from i in db.OrderItems.AsNoTracking()
            join o in db.Orders.AsNoTracking() on i.OrderId equals o.Id
            where !i.IsDeleted &&
                  !o.IsDeleted &&
                  storeIds.Contains(i.VendorStoreId) &&
                  o.Status == OrderStatus.Delivered
            orderby o.CreatedAtUtc descending, i.Id
            select new VendorEarningLineDto(
                o.Id,
                o.CreatedAtUtc,
                i.Id,
                i.ProductName,
                i.VariantName,
                i.Quantity,
                i.LineTotal,
                i.CommissionPercent,
                i.CommissionAmount,
                i.VendorNetAmount);

        return await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }
}
