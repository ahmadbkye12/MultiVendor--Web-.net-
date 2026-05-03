using Application.Authorization;

namespace UnitTests;

public class AuthPoliciesTests
{
    [Fact]
    public void Policy_constants_match_contract()
    {
        Assert.Equal("OnlyAdmin", AuthPolicies.OnlyAdmin);
        Assert.Equal("OnlyVendor", AuthPolicies.OnlyVendor);
        Assert.Equal("OnlyCustomer", AuthPolicies.OnlyCustomer);
        Assert.Equal("ApprovedVendorOnly", AuthPolicies.ApprovedVendorOnly);
    }
}
