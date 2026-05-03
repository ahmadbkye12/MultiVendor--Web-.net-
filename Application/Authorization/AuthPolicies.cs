namespace Application.Authorization;

public static class AuthPolicies
{
    public const string OnlyAdmin = nameof(OnlyAdmin);
    public const string OnlyVendor = nameof(OnlyVendor);
    public const string OnlyCustomer = nameof(OnlyCustomer);
    public const string ApprovedVendorOnly = nameof(ApprovedVendorOnly);
}
