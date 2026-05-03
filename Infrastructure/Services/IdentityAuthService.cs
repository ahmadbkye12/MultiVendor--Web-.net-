using Application.Auth;
using Application.Authorization;
using Application.Common;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    JwtTokenService jwt,
    IEmailSender emailSender,
    ILogger<IdentityAuthService> logger) : IIdentityAuthService
{
    public async Task<AuthOperationResult> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
            return AuthOperationResult.Failure(create.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, AuthRoles.Customer);

        db.Carts.Add(new Cart { CustomerUserId = user.Id });
        await db.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> RegisterVendorAsync(RegisterVendorRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
            return AuthOperationResult.Failure(create.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, AuthRoles.Vendor);

        db.Vendors.Add(new Vendor
        {
            OwnerUserId = user.Id,
            BusinessName = request.BusinessName,
            IsApproved = false
        });

        await db.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return AuthOperationResult.Failure("Invalid email or password.");

        var valid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            return AuthOperationResult.Failure("Invalid email or password.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return AuthOperationResult.Failure("Refresh token is required.");

        var hash = jwt.HashRefreshToken(request.RefreshToken);

        var existing = await db.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.TokenHash == hash && t.RevokedAtUtc == null && !t.IsDeleted,
                cancellationToken);

        if (existing is null || existing.ExpiresAtUtc <= DateTime.UtcNow)
            return AuthOperationResult.Failure("Invalid refresh token.");

        var user = await userManager.FindByIdAsync(existing.UserId);
        if (user is null)
            return AuthOperationResult.Failure("User no longer exists.");

        var (raw, expiresAtUtc) = jwt.CreateRefreshTokenPayload();
        var newHash = jwt.HashRefreshToken(raw);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAtUtc = expiresAtUtc
        });

        await db.SaveChangesAsync(cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwt.CreateAccessToken(user, roles);

        return AuthOperationResult.Success(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: raw,
            AccessTokenExpiresAtUtc: jwt.GetAccessTokenExpiryUtc(),
            RefreshTokenExpiresAtUtc: expiresAtUtc));
    }

    public async Task LogoutAsync(string userId, RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var hash = jwt.HashRefreshToken(request.RefreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(
            t => t.UserId == userId && t.TokenHash == hash && t.RevokedAtUtc == null && !t.IsDeleted,
            cancellationToken);

        if (token is null)
            return;

        token.RevokedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(bool EmailSent, IReadOnlyList<string> Errors)> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return (true, Array.Empty<string>());

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var subject = "Reset your password";
        var body =
            $"<p>Use this token to reset your password (valid for a limited time):</p>" +
            $"<pre style=\"word-break:break-all;\">{System.Net.WebUtility.HtmlEncode(token)}</pre>";

        try
        {
            await emailSender.SendAsync(user.Email!, subject, body, cancellationToken);
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Password reset email failed for {Email}", request.Email);
            return (false, new[] { "Unable to send reset email right now." });
        }
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return (false, new[] { "Unable to reset password." });

        var reset = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!reset.Succeeded)
            return (false, reset.Errors.Select(e => e.Description).ToArray());

        return (true, Array.Empty<string>());
    }

    private async Task<AuthOperationResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwt.CreateAccessToken(user, roles);

        var (rawRefresh, refreshExpires) = jwt.CreateRefreshTokenPayload();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = jwt.HashRefreshToken(rawRefresh),
            ExpiresAtUtc = refreshExpires
        });

        await db.SaveChangesAsync(cancellationToken);

        return AuthOperationResult.Success(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: rawRefresh,
            AccessTokenExpiresAtUtc: jwt.GetAccessTokenExpiryUtc(),
            RefreshTokenExpiresAtUtc: refreshExpires));
    }
}
