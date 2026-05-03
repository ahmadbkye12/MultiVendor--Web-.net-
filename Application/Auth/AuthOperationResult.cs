namespace Application.Auth;

public sealed class AuthOperationResult
{
    public bool Succeeded { get; init; }
    public TokenResponse? Tokens { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthOperationResult Success(TokenResponse tokens) =>
        new() { Succeeded = true, Tokens = tokens };

    public static AuthOperationResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static AuthOperationResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToArray() };
}
