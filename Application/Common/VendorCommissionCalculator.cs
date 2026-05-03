namespace Application.Common;

public static class VendorCommissionCalculator
{
    /// <summary>Round-half-away-from-zero line split between platform commission and vendor net.</summary>
    public static (decimal CommissionAmount, decimal VendorNetAmount) Split(decimal lineTotal, decimal commissionPercent)
    {
        var commission = Math.Round(lineTotal * commissionPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var net = Math.Round(lineTotal - commission, 2, MidpointRounding.AwayFromZero);
        return (commission, net);
    }
}
