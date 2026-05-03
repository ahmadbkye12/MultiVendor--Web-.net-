namespace Application.Authorization;

public static class AuthPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string VendorOnly = nameof(VendorOnly);
    public const string CustomerOnly = nameof(CustomerOnly);
    public const string DeliveryOnly = nameof(DeliveryOnly);

    public const string AdminOrVendor = nameof(AdminOrVendor);
    public const string AuthenticatedCustomer = nameof(AuthenticatedCustomer);
}
