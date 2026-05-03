using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasIndex(v => v.OwnerUserId).IsUnique();
        builder.Property(v => v.BusinessName).HasMaxLength(256);
        builder.Property(v => v.TaxNumber).HasMaxLength(64);

        builder.HasOne<ApplicationUser>()
            .WithOne(u => u.OwnedVendor)
            .HasForeignKey<Vendor>(v => v.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VendorStoreConfiguration : IEntityTypeConfiguration<VendorStore>
{
    public void Configure(EntityTypeBuilder<VendorStore> builder)
    {
        builder.ToTable("VendorStores");
        builder.Property(s => s.Name).HasMaxLength(256);
        builder.Property(s => s.Slug).HasMaxLength(256);
        builder.HasIndex(s => new { s.VendorId, s.Slug }).IsUnique();

        builder.HasOne(s => s.Vendor)
            .WithMany(v => v.Stores)
            .HasForeignKey(s => s.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(c => c.Name).HasMaxLength(256);
        builder.Property(c => c.Slug).HasMaxLength(256);
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.Property(p => p.Name).HasMaxLength(512);
        builder.Property(p => p.Slug).HasMaxLength(512);
        builder.HasIndex(p => new { p.VendorStoreId, p.Slug }).IsUnique();

        builder.HasOne(p => p.VendorStore)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.VendorStoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.Property(i => i.Url).HasMaxLength(2048);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.Property(v => v.Sku).HasMaxLength(128);
        builder.Property(v => v.Name).HasMaxLength(256);
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasIndex(c => c.CustomerUserId).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.CustomerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasIndex(i => new { i.CartId, i.ProductVariantId }).IsUnique();

        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany(v => v.CartItems)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.CustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.Property(i => i.ProductName).HasMaxLength(512);
        builder.Property(i => i.VariantName).HasMaxLength(256);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.Property(p => p.Provider).HasMaxLength(128);
        builder.Property(p => p.ExternalPaymentId).HasMaxLength(512);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.Property(r => r.Comment).HasMaxLength(4000);
        builder.HasIndex(r => new { r.ProductId, r.CustomerUserId }).IsUnique();

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.CustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("Wishlists");
        builder.HasIndex(w => new { w.CustomerUserId, w.ProductId }).IsUnique();

        builder.HasOne(w => w.Product)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(w => w.CustomerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.Property(c => c.Code).HasMaxLength(64);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasOne(c => c.VendorStore)
            .WithMany(s => s.Coupons)
            .HasForeignKey(c => c.VendorStoreId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.Property(a => a.Label).HasMaxLength(128);
        builder.Property(a => a.Line1).HasMaxLength(256);
        builder.Property(a => a.Line2).HasMaxLength(256);
        builder.Property(a => a.City).HasMaxLength(128);
        builder.Property(a => a.State).HasMaxLength(128);
        builder.Property(a => a.PostalCode).HasMaxLength(32);
        builder.Property(a => a.Country).HasMaxLength(128);
        builder.Property(a => a.Phone).HasMaxLength(32);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.Property(s => s.Carrier).HasMaxLength(128);
        builder.Property(s => s.TrackingNumber).HasMaxLength(256);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.Property(n => n.Title).HasMaxLength(256);
        builder.Property(n => n.Body).HasMaxLength(4000);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(a => a.EntityName).HasMaxLength(256);
        builder.Property(a => a.EntityId).HasMaxLength(128);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.Property(r => r.TokenHash).HasMaxLength(512);
        builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(512);
        builder.HasIndex(r => new { r.UserId, r.TokenHash });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
