using System.Text;
using Application.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        var signingKey = jwt["SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is required (min ~32 chars for HS256).");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminOnly, p => p.RequireRole(AuthRoles.Admin));
            options.AddPolicy(AuthPolicies.VendorOnly, p => p.RequireRole(AuthRoles.Vendor));
            options.AddPolicy(AuthPolicies.CustomerOnly, p => p.RequireRole(AuthRoles.Customer));
            options.AddPolicy(AuthPolicies.DeliveryOnly, p => p.RequireRole(AuthRoles.Delivery));
            options.AddPolicy(AuthPolicies.AdminOrVendor, p => p.RequireRole(AuthRoles.Admin, AuthRoles.Vendor));
            options.AddPolicy(AuthPolicies.AuthenticatedCustomer, p => p.RequireRole(AuthRoles.Customer));
        });

        return services;
    }
}
