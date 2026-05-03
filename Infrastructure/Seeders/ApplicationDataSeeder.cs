using Application.Authorization;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ApplicationDataSeeder
{
    private const string DemoPassword = "Demo123!";

    private static readonly Guid CatElectronicsId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid CatFashionId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    private static readonly Guid CatPhonesId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    private static readonly Guid CatMensClothingId = Guid.Parse("11111111-1111-1111-1111-111111111104");

    private static readonly Guid DemoVendorId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    private static readonly Guid DemoStoreId = Guid.Parse("22222222-2222-2222-2222-222222222201");

    private static readonly Guid ProductPhoneId = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private static readonly Guid ProductTeeId = Guid.Parse("44444444-4444-4444-4444-444444444402");

    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        await SeedDemoUsersAsync(db, userManager);
        await SeedCatalogAsync(db);
    }

    private static async Task SeedDemoUsersAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        await EnsureUserAsync(
            userManager,
            db,
            email: "admin@demo.local",
            firstName: "System",
            lastName: "Admin",
            role: AuthRoles.Admin);

        var vendorUser = await EnsureUserAsync(
            userManager,
            db,
            email: "vendor@demo.local",
            firstName: "Demo",
            lastName: "Vendor",
            role: AuthRoles.Vendor);

        if (!await db.Vendors.IgnoreQueryFilters().AnyAsync(v => v.OwnerUserId == vendorUser.Id))
        {
            db.Vendors.Add(new Vendor
            {
                Id = DemoVendorId,
                OwnerUserId = vendorUser.Id,
                BusinessName = "Demo Gadgets LLC",
                TaxNumber = "TAX-DEMO-001",
                IsApproved = true
            });

            db.VendorStores.Add(new VendorStore
            {
                Id = DemoStoreId,
                VendorId = DemoVendorId,
                Name = "Demo Gadgets Store",
                Slug = "demo-gadgets",
                Description = "Seed store for development and demos.",
                IsActive = true
            });

            await db.SaveChangesAsync();
        }

        var customer = await EnsureUserAsync(
            userManager,
            db,
            email: "customer@demo.local",
            firstName: "Demo",
            lastName: "Customer",
            role: AuthRoles.Customer);

        if (!await db.Carts.IgnoreQueryFilters().AnyAsync(c => c.CustomerUserId == customer.Id))
        {
            db.Carts.Add(new Cart { CustomerUserId = customer.Id });
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        string email,
        string firstName,
        string lastName,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create seed user '{email}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext db)
    {
        if (await db.Categories.IgnoreQueryFilters().AnyAsync())
            return;

        var now = DateTime.UtcNow;

        db.Categories.AddRange(
            new Category
            {
                Id = CatElectronicsId,
                Name = "Electronics",
                Slug = "electronics",
                DisplayOrder = 1,
                ParentCategoryId = null,
                CreatedAtUtc = now
            },
            new Category
            {
                Id = CatFashionId,
                Name = "Fashion",
                Slug = "fashion",
                DisplayOrder = 2,
                ParentCategoryId = null,
                CreatedAtUtc = now
            },
            new Category
            {
                Id = CatPhonesId,
                Name = "Phones",
                Slug = "phones",
                DisplayOrder = 1,
                ParentCategoryId = CatElectronicsId,
                CreatedAtUtc = now
            },
            new Category
            {
                Id = CatMensClothingId,
                Name = "Men's Clothing",
                Slug = "mens-clothing",
                DisplayOrder = 1,
                ParentCategoryId = CatFashionId,
                CreatedAtUtc = now
            });

        db.Products.AddRange(
            new Product
            {
                Id = ProductPhoneId,
                VendorStoreId = DemoStoreId,
                CategoryId = CatPhonesId,
                Name = "Demo Smartphone X",
                Slug = "demo-smartphone-x",
                Description = "Sample unlocked smartphone listing.",
                BasePrice = 699.99m,
                IsPublished = true,
                ApprovalStatus = ProductApprovalStatus.Approved,
                CreatedAtUtc = now
            },
            new Product
            {
                Id = ProductTeeId,
                VendorStoreId = DemoStoreId,
                CategoryId = CatMensClothingId,
                Name = "Classic Cotton Tee",
                Slug = "classic-cotton-tee",
                Description = "Comfortable everyday tee.",
                BasePrice = 24.99m,
                IsPublished = true,
                ApprovalStatus = ProductApprovalStatus.Approved,
                CreatedAtUtc = now
            });

        db.ProductVariants.AddRange(
            new ProductVariant
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555501"),
                ProductId = ProductPhoneId,
                Sku = "PHONE-X-128-BLK",
                Name = "128GB / Black",
                Price = 699.99m,
                StockQuantity = 50,
                CreatedAtUtc = now
            },
            new ProductVariant
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555502"),
                ProductId = ProductPhoneId,
                Sku = "PHONE-X-256-WHT",
                Name = "256GB / White",
                Price = 749.99m,
                StockQuantity = 30,
                CreatedAtUtc = now
            },
            new ProductVariant
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555503"),
                ProductId = ProductTeeId,
                Sku = "TEE-M-BLK",
                Name = "Medium / Black",
                Price = 24.99m,
                StockQuantity = 120,
                CreatedAtUtc = now
            },
            new ProductVariant
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555504"),
                ProductId = ProductTeeId,
                Sku = "TEE-L-GRY",
                Name = "Large / Gray",
                Price = 24.99m,
                StockQuantity = 80,
                CreatedAtUtc = now
            });

        db.ProductImages.AddRange(
            new ProductImage
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                ProductId = ProductPhoneId,
                Url = "https://via.placeholder.com/800x600.png?text=Demo+Phone",
                SortOrder = 1,
                CreatedAtUtc = now
            },
            new ProductImage
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666602"),
                ProductId = ProductTeeId,
                Url = "https://via.placeholder.com/800x600.png?text=Demo+Tee",
                SortOrder = 1,
                CreatedAtUtc = now
            });

        db.Coupons.Add(new Coupon
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777701"),
            Code = "DEMO10",
            DiscountType = CouponDiscountType.Percentage,
            DiscountValue = 10,
            MaxUses = 1000,
            UsedCount = 0,
            ExpiresAtUtc = now.AddYears(1),
            IsActive = true,
            VendorStoreId = DemoStoreId,
            CreatedAtUtc = now
        });

        await db.SaveChangesAsync();
    }
}
