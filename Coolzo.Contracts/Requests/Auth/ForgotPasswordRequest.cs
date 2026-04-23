namespace Coolzo.Contracts.Requests.Auth;

public sealed class ForgotPasswordRequest
{
    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? LoginId { get; init; }
}
