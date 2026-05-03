using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Shipment : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public ShipmentStatus Status { get; set; }

    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}
