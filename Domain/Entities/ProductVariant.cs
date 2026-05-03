using Domain.Common;

namespace Domain.Entities;

public class ProductVariant : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
