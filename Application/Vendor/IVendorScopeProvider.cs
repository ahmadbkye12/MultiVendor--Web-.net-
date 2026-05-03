namespace Application.Vendor;

public sealed record VendorScope(Guid VendorId, IReadOnlyList<Guid> StoreIds, decimal DefaultCommissionPercent);

public interface IVendorScopeProvider
{
    Task<VendorScope?> GetScopeAsync(CancellationToken cancellationToken = default);
}
