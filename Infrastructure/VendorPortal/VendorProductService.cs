using Application.Common;
using Application.Vendor;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.VendorPortal;

public sealed class VendorProductService(ApplicationDbContext db, IVendorScopeProvider scopeProvider) : IVendorProductService
{
    public async Task<IReadOnlyList<VendorProductListItemDto>> ListAsync(Guid? vendorStoreId, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var storeIds = ResolveStoreFilter(scope, vendorStoreId);

        var query = db.Products.AsNoTracking()
            .Where(p => storeIds.Contains(p.VendorStoreId) && !p.IsDeleted)
            .Include(p => p.Variants);

        var list = await query.OrderByDescending(p => p.CreatedAtUtc).ToListAsync(cancellationToken);

        return list.Select(p =>
        {
            var variants = p.Variants.Where(v => !v.IsDeleted).ToList();
            return new VendorProductListItemDto(
                p.Id,
                p.VendorStoreId,
                p.Name,
                p.Slug,
                p.BasePrice,
                p.IsPublished,
                p.ApprovalStatus,
                variants.Count,
                variants.Sum(v => v.StockQuantity),
                p.CreatedAtUtc);
        }).ToList();
    }

    public async Task<VendorProductDetailDto?> GetAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var product = await db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(
                p => p.Id == productId && scope.StoreIds.Contains(p.VendorStoreId) && !p.IsDeleted,
                cancellationToken);

        return product is null ? null : MapDetail(product);
    }

    public async Task<VendorProductDetailDto> CreateAsync(CreateVendorProductRequest request, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        if (!scope.StoreIds.Contains(request.VendorStoreId))
            throw new InvalidOperationException("Store is not owned by this vendor.");

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Slugify(request.Name)
            : SlugHelper.Slugify(request.Slug);

        var slug = await EnsureUniqueSlugAsync(request.VendorStoreId, baseSlug, excludeProductId: null, cancellationToken);

        var product = new Product
        {
            VendorStoreId = request.VendorStoreId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BasePrice = request.BasePrice,
            IsPublished = request.IsPublished,
            Slug = slug,
            ApprovalStatus = ProductApprovalStatus.Pending
        };

        db.Products.Add(product);

        foreach (var v in request.Variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Sku = v.Sku.Trim(),
                Name = v.Name?.Trim(),
                Price = v.Price,
                StockQuantity = v.StockQuantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return (await GetAsync(product.Id, cancellationToken))!;
    }

    public async Task<VendorProductDetailDto?> UpdateAsync(Guid productId, UpdateVendorProductRequest request, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var product = await db.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.Id == productId && scope.StoreIds.Contains(p.VendorStoreId) && !p.IsDeleted,
                cancellationToken);

        if (product is null)
            return null;

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Slugify(request.Name)
            : SlugHelper.Slugify(request.Slug);

        product.Slug = await EnsureUniqueSlugAsync(product.VendorStoreId, baseSlug, product.Id, cancellationToken);
        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.BasePrice = request.BasePrice;
        product.IsPublished = request.IsPublished;

        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(product.Id, cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var product = await db.Products.FirstOrDefaultAsync(
            p => p.Id == productId && scope.StoreIds.Contains(p.VendorStoreId) && !p.IsDeleted,
            cancellationToken);

        if (product is null)
            return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VendorProductDetailDto?> AddImageAsync(Guid productId, string url, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var product = await db.Products.FirstOrDefaultAsync(
            p => p.Id == productId && scope.StoreIds.Contains(p.VendorStoreId) && !p.IsDeleted,
            cancellationToken);

        if (product is null)
            return null;

        var maxSort = await db.ProductImages
            .Where(i => i.ProductId == productId && !i.IsDeleted)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        db.ProductImages.Add(new ProductImage
        {
            ProductId = productId,
            Url = url.Trim(),
            SortOrder = maxSort + 1
        });

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(productId, cancellationToken);
    }

    public async Task<bool> SoftDeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var image = await db.ProductImages
            .Include(i => i.Product)
            .FirstOrDefaultAsync(
                i => i.Id == imageId && i.ProductId == productId && !i.IsDeleted &&
                     scope.StoreIds.Contains(i.Product.VendorStoreId) && !i.Product.IsDeleted,
                cancellationToken);

        if (image is null)
            return false;

        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VendorProductDetailDto?> UpdateVariantStockAsync(
        Guid productId,
        Guid variantId,
        int stockQuantity,
        CancellationToken cancellationToken = default)
    {
        var scope = await scopeProvider.GetScopeAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Vendor scope could not be resolved.");

        var variant = await db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(
                v => v.Id == variantId && v.ProductId == productId && !v.IsDeleted &&
                     scope.StoreIds.Contains(v.Product.VendorStoreId) && !v.Product.IsDeleted,
                cancellationToken);

        if (variant is null)
            return null;

        variant.StockQuantity = stockQuantity;
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(productId, cancellationToken);
    }

    private static List<Guid> ResolveStoreFilter(VendorScope scope, Guid? vendorStoreId)
    {
        if (vendorStoreId is null)
            return scope.StoreIds.ToList();

        if (!scope.StoreIds.Contains(vendorStoreId.Value))
            throw new InvalidOperationException("Store is not owned by this vendor.");

        return [vendorStoreId.Value];
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var exists = await db.Categories.AnyAsync(c => c.Id == categoryId && !c.IsDeleted, cancellationToken);
        if (!exists)
            throw new InvalidOperationException("Category was not found.");
    }

    private async Task<string> EnsureUniqueSlugAsync(
        Guid vendorStoreId,
        string baseSlug,
        Guid? excludeProductId,
        CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        var suffix = 0;
        while (await db.Products.AnyAsync(
                   p =>
                       p.VendorStoreId == vendorStoreId &&
                       !p.IsDeleted &&
                       p.Slug == slug &&
                       (excludeProductId == null || p.Id != excludeProductId.Value),
                   cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static VendorProductDetailDto MapDetail(Product product)
    {
        var variants = product.Variants.Where(v => !v.IsDeleted)
            .OrderBy(v => v.Sku)
            .Select(v => new VendorProductVariantDto(v.Id, v.Sku, v.Name, v.Price, v.StockQuantity))
            .ToList();

        var images = product.Images.Where(i => !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .Select(i => new VendorProductImageDto(i.Id, i.Url, i.SortOrder))
            .ToList();

        return new VendorProductDetailDto(
            product.Id,
            product.VendorStoreId,
            product.CategoryId,
            product.Category.Name,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice,
            product.IsPublished,
            product.ApprovalStatus,
            variants,
            images);
    }
}
