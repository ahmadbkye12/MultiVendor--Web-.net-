using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Order : BaseAuditableEntity
{
    public string CustomerUserId { get; set; } = string.Empty;

    public Guid? ShippingAddressId { get; set; }
    public Address? ShippingAddress { get; set; }

    public Guid? BillingAddressId { get; set; }
    public Address? BillingAddress { get; set; }

    public OrderStatus Status { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
