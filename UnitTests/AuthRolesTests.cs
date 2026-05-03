using Application.Authorization;

namespace UnitTests;

public class AuthRolesTests
{
    [Fact]
    public void All_roles_contains_four_expected_names()
    {
        Assert.Contains(AuthRoles.Admin, AuthRoles.All);
        Assert.Contains(AuthRoles.Vendor, AuthRoles.All);
        Assert.Contains(AuthRoles.Customer, AuthRoles.All);
        Assert.Contains(AuthRoles.Delivery, AuthRoles.All);
        Assert.Equal(4, AuthRoles.All.Count);
    }
}
