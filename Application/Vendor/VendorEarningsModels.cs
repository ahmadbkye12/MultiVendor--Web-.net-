namespace Application.Vendor;

public sealed record VendorEarningsSummaryDto(
    decimal TotalVendorNet,
    decimal TotalCommission,
    decimal TotalLineAmount,
    int CompletedOrderCount);

public sealed record VendorEarningLineDto(
    Guid OrderId,
    DateTime OrderPlacedAtUtc,
    Guid OrderItemId,
    string ProductName,
    string? VariantName,
    int Quantity,
    decimal LineTotal,
    decimal CommissionPercent,
    decimal CommissionAmount,
    decimal VendorNetAmount);
