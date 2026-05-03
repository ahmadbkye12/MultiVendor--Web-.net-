using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations;

internal static class IdentityTablesExtensions
{
    public static void ConfigureIdentityTables(this ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(128);
            entity.Property(u => u.LastName).HasMaxLength(128);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(128);
            entity.Property(r => r.NormalizedName).HasMaxLength(128);
        });
    }
}
