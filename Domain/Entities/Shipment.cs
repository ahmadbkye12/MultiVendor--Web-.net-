using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Shipment : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>Optional delivery/courier user assigned to this shipment.</summary>
    public string? AssignedDeliveryUserId { get; set; }

    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public ShipmentStatus Status { get; set; }

    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}
