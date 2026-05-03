using Domain.Enums;

namespace Application.Vendor;

public sealed record ProductVariantUpsertDto(
    string Sku,
    string? Name,
    decimal Price,
    int StockQuantity);

public sealed record CreateVendorProductRequest(
    Guid VendorStoreId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    bool IsPublished,
    string? Slug,
    IReadOnlyList<ProductVariantUpsertDto> Variants);

public sealed record UpdateVendorProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    bool IsPublished,
    string? Slug);

public sealed record UpdateVariantStockRequest(int StockQuantity);

public sealed record VendorProductListItemDto(
    Guid Id,
    Guid VendorStoreId,
    string Name,
    string Slug,
    decimal BasePrice,
    bool IsPublished,
    ProductApprovalStatus ApprovalStatus,
    int VariantCount,
    int TotalStock,
    DateTime CreatedAtUtc);

public sealed record VendorProductVariantDto(
    Guid Id,
    string Sku,
    string? Name,
    decimal Price,
    int StockQuantity);

public sealed record VendorProductImageDto(Guid Id, string Url, int SortOrder);

public sealed record VendorProductDetailDto(
    Guid Id,
    Guid VendorStoreId,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    bool IsPublished,
    ProductApprovalStatus ApprovalStatus,
    IReadOnlyList<VendorProductVariantDto> Variants,
    IReadOnlyList<VendorProductImageDto> Images);
