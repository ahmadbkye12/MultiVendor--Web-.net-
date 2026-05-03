using Domain.Enums;

namespace Application.Vendor;

public interface IVendorDashboardService
{
    Task<VendorDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public interface IVendorProductService
{
    Task<IReadOnlyList<VendorProductListItemDto>> ListAsync(Guid? vendorStoreId, CancellationToken cancellationToken = default);

    Task<VendorProductDetailDto?> GetAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<VendorProductDetailDto> CreateAsync(CreateVendorProductRequest request, CancellationToken cancellationToken = default);

    Task<VendorProductDetailDto?> UpdateAsync(Guid productId, UpdateVendorProductRequest request, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<VendorProductDetailDto?> AddImageAsync(Guid productId, string url, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);

    Task<VendorProductDetailDto?> UpdateVariantStockAsync(Guid productId, Guid variantId, int stockQuantity, CancellationToken cancellationToken = default);
}

public interface IVendorOrderService
{
    Task<IReadOnlyList<VendorOrderListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<VendorOrderDetailDto?> GetAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<VendorOrderDetailDto?> UpdateItemStatusAsync(Guid orderId, Guid itemId, VendorOrderItemStatus status, CancellationToken cancellationToken = default);
}

public interface IVendorReviewService
{
    Task<IReadOnlyList<VendorReviewListItemDto>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
}

public interface IVendorEarningsService
{
    Task<VendorEarningsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VendorEarningLineDto>> ListLinesAsync(int skip, int take, CancellationToken cancellationToken = default);
}

public interface IVendorStoreService
{
    Task<IReadOnlyList<VendorStoreSummaryDto>> ListStoresAsync(CancellationToken cancellationToken = default);

    Task<VendorStoreSummaryDto?> UpdateStoreAsync(Guid storeId, UpdateVendorStoreRequest request, CancellationToken cancellationToken = default);
}
