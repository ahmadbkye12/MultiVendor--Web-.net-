using Application.Auth;
using Application.Authorization;
using Application.Common;
using Application.Vendor;
using Infrastructure.Authorization;
using Infrastructure.Email;
using Infrastructure.Identity;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.VendorPortal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Configure it in appsettings.json.");

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddHttpContextAccessor();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.OnlyAdmin, p => p.RequireRole(AuthRoles.Admin));
            options.AddPolicy(AuthPolicies.OnlyVendor, p => p.RequireRole(AuthRoles.Vendor));
            options.AddPolicy(AuthPolicies.OnlyCustomer, p => p.RequireRole(AuthRoles.Customer));
            options.AddPolicy(AuthPolicies.ApprovedVendorOnly, p =>
            {
                p.RequireAuthenticatedUser();
                p.AddRequirements(new ApprovedVendorRequirement());
            });
        });

        services.AddScoped<IAuthorizationHandler, ApprovedVendorAuthorizationHandler>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IIdentityAuthService, IdentityAuthService>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        services.AddScoped<IVendorScopeProvider, VendorScopeProvider>();
        services.AddScoped<IVendorDashboardService, VendorDashboardService>();
        services.AddScoped<IVendorProductService, VendorProductService>();
        services.AddScoped<IVendorOrderService, VendorOrderService>();
        services.AddScoped<IVendorReviewService, VendorReviewService>();
        services.AddScoped<IVendorEarningsService, VendorEarningsService>();
        services.AddScoped<IVendorStoreService, VendorStoreService>();

        return services;
    }
}
