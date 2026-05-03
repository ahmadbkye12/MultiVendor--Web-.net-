using Application.Vendor;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorDashboardService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorDashboardService
{
    public async Task<VendorDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = scope.StoreIds;
        if (storeIds.Count == 0)
        {
            return new VendorDashboardSummaryDto(
                TotalProducts: 0,
                PendingProducts: 0,
                ApprovedProducts: 0,
                TotalOrders: 0,
                TotalEarnings: 0,
                LatestOrders: Array.Empty<VendorLatestOrderDto>());
        }

        var storeIdSet = storeIds.ToHashSet();

        var totalProducts = await db.Products.CountAsync(
            p => storeIdSet.Contains(p.VendorStoreId) && !p.IsDeleted,
            cancellationToken);

        var pendingProducts = await db.Products.CountAsync(
            p => storeIdSet.Contains(p.VendorStoreId) && !p.IsDeleted && p.ApprovalStatus == ProductApprovalStatus.Pending,
            cancellationToken);

        var approvedProducts = await db.Products.CountAsync(
            p => storeIdSet.Contains(p.VendorStoreId) && !p.IsDeleted && p.ApprovalStatus == ProductApprovalStatus.Approved,
            cancellationToken);

        var totalOrders = await db.Orders.CountAsync(
            o => !o.IsDeleted &&
                 o.Items.Any(i => !i.IsDeleted && storeIdSet.Contains(i.VendorStoreId)),
            cancellationToken);

        var totalEarnings = await db.OrderItems
            .Where(i =>
                !i.IsDeleted &&
                storeIdSet.Contains(i.VendorStoreId) &&
                !i.Order.IsDeleted &&
                i.Order.Status == OrderStatus.Delivered)
            .SumAsync(i => i.VendorNetAmount, cancellationToken);

        var latestOrdersEntities = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o =>
                !o.IsDeleted &&
                o.Items.Any(i => !i.IsDeleted && storeIdSet.Contains(i.VendorStoreId)))
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        var latest = latestOrdersEntities.Select(o =>
        {
            var lines = o.Items.Where(i => !i.IsDeleted && storeIdSet.Contains(i.VendorStoreId)).ToList();
            var vendorSubtotal = lines.Sum(i => i.LineTotal);
            return new VendorLatestOrderDto(o.Id, o.CreatedAtUtc, o.Status, vendorSubtotal, lines.Count);
        }).ToList();

        return new VendorDashboardSummaryDto(
            totalProducts,
            pendingProducts,
            approvedProducts,
            totalOrders,
            totalEarnings,
            latest);
    }
}
