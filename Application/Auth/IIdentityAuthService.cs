namespace Application.Auth;

public interface IIdentityAuthService
{
    Task<AuthOperationResult> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default);

    Task<AuthOperationResult> RegisterVendorAsync(RegisterVendorRequest request, CancellationToken cancellationToken = default);

    Task<AuthOperationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthOperationResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(string userId, RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<(bool EmailSent, IReadOnlyList<string> Errors)> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IReadOnlyList<string> Errors)> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
