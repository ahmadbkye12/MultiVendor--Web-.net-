using Application.Authorization;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeders;

public static class IdentityRoleSeeder
{
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in AuthRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
