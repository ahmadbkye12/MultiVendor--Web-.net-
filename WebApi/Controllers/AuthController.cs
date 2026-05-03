using System.Security.Claims;
using Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IIdentityAuthService auth) : ControllerBase
{
    [HttpPost("register/customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterCustomerAsync(request, ct);
        return MapAuthResult(result);
    }

    [HttpPost("register/vendor")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterVendor([FromBody] RegisterVendorRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterVendorAsync(request, ct);
        return MapAuthResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return MapAuthResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await auth.RefreshAsync(request, ct);
        return MapAuthResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await auth.LogoutAsync(userId, request, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var (sent, errors) = await auth.ForgotPasswordAsync(request, ct);
        if (!sent)
            return BadRequest(errors);

        return Ok(new { message = "If the email exists, reset instructions were sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var (ok, errors) = await auth.ResetPasswordAsync(request, ct);
        if (!ok)
            return BadRequest(errors);

        return Ok(new { message = "Password has been reset." });
    }

    private IActionResult MapAuthResult(AuthOperationResult result)
    {
        if (result.Succeeded && result.Tokens is not null)
            return Ok(result.Tokens);

        return BadRequest(result.Errors);
    }
}
