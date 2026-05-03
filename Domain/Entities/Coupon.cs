using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Coupon : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? VendorStoreId { get; set; }
    public VendorStore? VendorStore { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
