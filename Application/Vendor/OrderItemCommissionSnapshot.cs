using Application.Common;
using Domain.Entities;

namespace Application.Vendor;

/// <summary>Used at checkout when creating order lines so commission is frozen for payouts.</summary>
public static class OrderItemCommissionSnapshot
{
    public static void Apply(OrderItem item, Guid vendorStoreId, decimal commissionPercent)
    {
        item.VendorStoreId = vendorStoreId;
        item.LineTotal = Math.Round(item.Quantity * item.UnitPrice, 2, MidpointRounding.AwayFromZero);
        item.CommissionPercent = commissionPercent;
        var (commission, net) = VendorCommissionCalculator.Split(item.LineTotal, commissionPercent);
        item.CommissionAmount = commission;
        item.VendorNetAmount = net;
    }
}
