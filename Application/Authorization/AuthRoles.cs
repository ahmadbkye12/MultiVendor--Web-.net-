namespace Application.Authorization;

public static class AuthRoles
{
    public const string Admin = nameof(Admin);
    public const string Vendor = nameof(Vendor);
    public const string Customer = nameof(Customer);
    public const string Delivery = nameof(Delivery);

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Admin,
        Vendor,
        Customer,
        Delivery
    };
}
