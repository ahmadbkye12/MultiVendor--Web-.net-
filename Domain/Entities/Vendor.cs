using Domain.Common;

namespace Domain.Entities;

public class Vendor : BaseAuditableEntity
{
    public string OwnerUserId { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public bool IsApproved { get; set; }

    public ICollection<VendorStore> Stores { get; set; } = new List<VendorStore>();
}
