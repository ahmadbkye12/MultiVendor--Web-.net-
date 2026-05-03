using Domain.Enums;

namespace Application.Vendor;

public sealed record VendorDashboardSummaryDto(
    int TotalProducts,
    int PendingProducts,
    int ApprovedProducts,
    int TotalOrders,
    decimal TotalEarnings,
    IReadOnlyList<VendorLatestOrderDto> LatestOrders);

public sealed record VendorLatestOrderDto(
    Guid OrderId,
    DateTime CreatedAtUtc,
    OrderStatus OrderStatus,
    decimal VendorSubtotal,
    int LineCount);
