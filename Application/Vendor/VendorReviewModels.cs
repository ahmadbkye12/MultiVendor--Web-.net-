namespace Application.Vendor;

public sealed record VendorReviewListItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string CustomerUserId,
    int Rating,
    string? Comment,
    DateTime CreatedAtUtc);
