using Domain.Common;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorStore> VendorStores => Set<VendorStore>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> Wishlists => Set<WishlistItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureIdentityTables();
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.ApplySoftDeleteFilters();

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType != typeof(decimal) && property.ClrType != typeof(decimal?))
                    continue;
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.CreateVersion7();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
