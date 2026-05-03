using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await IdentityRoleSeeder.SeedAsync(roleManager);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await ApplicationDataSeeder.SeedAsync(db, userManager);
    }
}
