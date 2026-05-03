using Application.Vendor;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorStoreService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorStoreService
{
    public async Task<IReadOnlyList<VendorStoreSummaryDto>> ListStoresAsync(CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        return await db.VendorStores.AsNoTracking()
            .Where(s => s.VendorId == scope.VendorId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new VendorStoreSummaryDto(
                s.Id,
                s.Name,
                s.Slug,
                s.IsActive,
                s.LogoUrl,
                s.BannerUrl,
                s.Description,
                s.ContactEmail,
                s.ContactPhone))
            .ToListAsync(cancellationToken);
    }

    public async Task<VendorStoreSummaryDto?> UpdateStoreAsync(Guid storeId, UpdateVendorStoreRequest request, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var store = await db.VendorStores.FirstOrDefaultAsync(
            s => s.Id == storeId && s.VendorId == scope.VendorId && !s.IsDeleted,
            cancellationToken);

        if (store is null)
            return null;

        store.Name = request.Name.Trim();
        store.Description = request.Description?.Trim();
        store.ContactEmail = request.ContactEmail?.Trim();
        store.ContactPhone = request.ContactPhone?.Trim();
        store.IsActive = request.IsActive;
        store.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        store.BannerUrl = string.IsNullOrWhiteSpace(request.BannerUrl) ? null : request.BannerUrl.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return new VendorStoreSummaryDto(
            store.Id,
            store.Name,
            store.Slug,
            store.IsActive,
            store.LogoUrl,
            store.BannerUrl,
            store.Description,
            store.ContactEmail,
            store.ContactPhone);
    }
}
