using Application.Vendor;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorOrderService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorOrderService
{
    public async Task<IReadOnlyList<VendorOrderListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = scope.StoreIds.ToHashSet();
        if (storeIds.Count == 0)
            return [];

        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted && o.Items.Any(i => !i.IsDeleted && storeIds.Contains(i.VendorStoreId)))
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(500)
            .ToListAsync(cancellationToken);

        return orders.Select(o =>
        {
            var lines = o.Items.Where(i => !i.IsDeleted && storeIds.Contains(i.VendorStoreId)).ToList();
            return new VendorOrderListItemDto(
                o.Id,
                o.CreatedAtUtc,
                o.Status,
                lines.Sum(i => i.LineTotal),
                lines.Count);
        }).ToList();
    }

    public async Task<VendorOrderDetailDto?> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = scope.StoreIds.ToHashSet();

        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order is null)
            return null;

        var lines = order.Items.Where(i => !i.IsDeleted && storeIds.Contains(i.VendorStoreId)).ToList();
        if (lines.Count == 0)
            return null;

        var dtos = lines
            .OrderBy(i => i.ProductName)
            .Select(i => new VendorOrderItemDetailDto(
                i.Id,
                i.ProductName,
                i.VariantName,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal,
                i.CommissionPercent,
                i.CommissionAmount,
                i.VendorNetAmount,
                i.VendorFulfillmentStatus))
            .ToList();

        return new VendorOrderDetailDto(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            lines.Sum(i => i.LineTotal),
            lines.Sum(i => i.CommissionAmount),
            lines.Sum(i => i.VendorNetAmount),
            dtos);
    }

    public async Task<VendorOrderDetailDto?> UpdateItemStatusAsync(
        Guid orderId,
        Guid itemId,
        VendorOrderItemStatus status,
        CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = scope.StoreIds.ToHashSet();

        var item = await db.OrderItems
            .Include(i => i.Order)
            .FirstOrDefaultAsync(
                i => i.Id == itemId && i.OrderId == orderId && !i.IsDeleted && !i.Order.IsDeleted,
                cancellationToken);

        if (item is null || !storeIds.Contains(item.VendorStoreId))
            return null;

        item.VendorFulfillmentStatus = status;
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(orderId, cancellationToken);
    }
}
