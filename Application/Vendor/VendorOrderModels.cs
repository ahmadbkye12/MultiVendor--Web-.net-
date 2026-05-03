using Domain.Enums;

namespace Application.Vendor;

public sealed record VendorOrderListItemDto(
    Guid OrderId,
    DateTime CreatedAtUtc,
    OrderStatus OrderStatus,
    decimal VendorLineTotal,
    int ItemCount);

public sealed record VendorOrderItemDetailDto(
    Guid Id,
    string ProductName,
    string? VariantName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal CommissionPercent,
    decimal CommissionAmount,
    decimal VendorNetAmount,
    VendorOrderItemStatus VendorFulfillmentStatus);

public sealed record VendorOrderDetailDto(
    Guid OrderId,
    DateTime CreatedAtUtc,
    OrderStatus OrderStatus,
    decimal VendorItemsSubtotal,
    decimal VendorCommissionTotal,
    decimal VendorNetTotal,
    IReadOnlyList<VendorOrderItemDetailDto> Items);

public sealed record UpdateVendorOrderItemStatusRequest(VendorOrderItemStatus Status);
