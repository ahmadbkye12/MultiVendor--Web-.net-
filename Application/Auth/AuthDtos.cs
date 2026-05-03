namespace Application.Auth;

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName);

public sealed record RegisterVendorRequest(
    string Email,
    string Password,
    string BusinessName,
    string? FirstName,
    string? LastName);

public sealed record LoginRequest(string Email, string Password);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
