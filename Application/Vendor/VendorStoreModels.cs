namespace Application.Vendor;

public sealed record VendorStoreSummaryDto(
    Guid Id,
    string Name,
    string? Slug,
    bool IsActive,
    string? LogoUrl,
    string? BannerUrl,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);

public sealed record UpdateVendorStoreRequest(
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    string? LogoUrl,
    string? BannerUrl);
