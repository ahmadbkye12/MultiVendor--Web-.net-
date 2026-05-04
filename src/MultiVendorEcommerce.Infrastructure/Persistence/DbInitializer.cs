using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        foreach (var role in Roles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // ----- Admin -----
        var adminEmail = "admin@shop.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Site Administrator", EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        // ----- Categories -----
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Electronics", Slug = "electronics", DisplayOrder = 1, IsActive = true, IconUrl = "https://picsum.photos/seed/electronics/120/120" },
                new Category { Name = "Fashion",     Slug = "fashion",     DisplayOrder = 2, IsActive = true, IconUrl = "https://picsum.photos/seed/fashion/120/120" },
                new Category { Name = "Home",        Slug = "home",        DisplayOrder = 3, IsActive = true, IconUrl = "https://picsum.photos/seed/home/120/120" },
                new Category { Name = "Books",       Slug = "books",       DisplayOrder = 4, IsActive = true, IconUrl = "https://picsum.photos/seed/books/120/120" },
                new Category { Name = "Sports",      Slug = "sports",      DisplayOrder = 5, IsActive = true, IconUrl = "https://picsum.photos/seed/sports/120/120" }
            );
            await db.SaveChangesAsync();
        }

        // ----- Demo vendors (with stores + sample products) -----
        await SeedDemoVendorAsync(db, userManager,
            email: "techgear@shop.com", fullName: "Alex Vendor",
            businessName: "TechGear Co.", storeName: "TechGear",
            categorySlug: "electronics",
            products: new[]
            {
                new SeedProduct("Wireless Earbuds Pro", "Bluetooth 5.3, 24h battery, ANC.", 79.99m, "earbuds-pro"),
                new SeedProduct("Smart Watch X1", "AMOLED, GPS, heart-rate, 7-day battery.", 149.00m, "smart-watch-x1"),
                new SeedProduct("4K Action Camera", "Waterproof, 60fps, image stabilization.", 199.50m, "action-camera-4k"),
                new SeedProduct("Bluetooth Speaker Mini", "Portable, 12h playback, IPX5.", 39.90m, "bt-speaker-mini"),
                new SeedProduct("Mechanical Keyboard 75%", "Hot-swappable, RGB, USB-C.", 89.00m, "mech-keyboard-75")
            });

        await SeedDemoVendorAsync(db, userManager,
            email: "stylehub@shop.com", fullName: "Sara Vendor",
            businessName: "StyleHub LLC", storeName: "StyleHub",
            categorySlug: "fashion",
            products: new[]
            {
                new SeedProduct("Classic Leather Jacket", "Genuine leather, slim fit.", 129.00m, "leather-jacket-classic"),
                new SeedProduct("Cotton T-Shirt — 3 Pack", "Soft, breathable, multiple colors.", 24.99m, "cotton-tee-3pack"),
                new SeedProduct("Canvas Sneakers", "Casual everyday sneakers.", 49.50m, "canvas-sneakers"),
                new SeedProduct("Wool Beanie", "Warm and stretchy.", 14.00m, "wool-beanie"),
                new SeedProduct("Denim Backpack", "20L, padded laptop sleeve.", 59.95m, "denim-backpack")
            });

        // ----- Demo customer -----
        var buyerEmail = "demo@shop.com";
        if (await userManager.FindByEmailAsync(buyerEmail) is null)
        {
            var buyer = new ApplicationUser { UserName = buyerEmail, Email = buyerEmail, FullName = "Demo Customer", EmailConfirmed = true };
            await userManager.CreateAsync(buyer, "Demo123!");
            await userManager.AddToRoleAsync(buyer, Roles.Customer);
        }

        // ----- Sample platform coupon -----
        if (!await db.Coupons.AnyAsync(c => c.Code == "WELCOME10"))
        {
            db.Coupons.Add(new Coupon
            {
                Code = "WELCOME10",
                DiscountType = CouponDiscountType.Percentage,
                DiscountValue = 10m,
                MinimumOrderAmount = 0m,
                IsActive = true,
                VendorStoreId = null,
                StartsAtUtc = DateTime.UtcNow.AddDays(-1)
            });
            await db.SaveChangesAsync();
        }
    }

    private sealed record SeedProduct(string Name, string Description, decimal BasePrice, string Slug);

    private static async Task SeedDemoVendorAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        string email, string fullName,
        string businessName, string storeName,
        string categorySlug,
        IEnumerable<SeedProduct> products)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null) return;   // idempotent

        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true };
        var create = await userManager.CreateAsync(user, "Demo123!");
        if (!create.Succeeded) return;
        await userManager.AddToRoleAsync(user, Roles.Vendor);

        var slug = storeName.ToLowerInvariant().Replace(' ', '-');
        var vendor = new Vendor
        {
            OwnerUserId = user.Id,
            BusinessName = businessName,
            IsApproved = true,
            DefaultCommissionPercent = 10m
        };
        var store = new VendorStore
        {
            Vendor = vendor,
            Name = storeName,
            Slug = slug,
            Description = $"Welcome to {storeName} — handpicked products at fair prices.",
            IsActive = true,
            LogoUrl   = $"https://picsum.photos/seed/{slug}-logo/200/200",
            BannerUrl = $"https://picsum.photos/seed/{slug}-banner/1200/300",
            ContactEmail = email
        };
        db.Vendors.Add(vendor);
        db.VendorStores.Add(store);
        await db.SaveChangesAsync();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
        if (category is null) return;

        foreach (var p in products)
        {
            if (await db.Products.AnyAsync(x => x.Slug == p.Slug)) continue;

            var product = new Product
            {
                VendorStoreId = store.Id,
                CategoryId = category.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                BasePrice = p.BasePrice,
                IsPublished = true,
                ApprovalStatus = ProductApprovalStatus.Approved,
                Images = new List<ProductImage>
                {
                    new() { Url = $"https://picsum.photos/seed/{p.Slug}-1/600/400", IsMain = true,  SortOrder = 0 },
                    new() { Url = $"https://picsum.photos/seed/{p.Slug}-2/600/400", IsMain = false, SortOrder = 1 }
                },
                Variants = new List<ProductVariant>
                {
                    new() { Sku = $"{p.Slug.ToUpperInvariant()}-DEF", Name = "Default", Price = p.BasePrice, StockQuantity = 25, IsActive = true }
                }
            };
            db.Products.Add(product);
        }
        await db.SaveChangesAsync();
    }
}
