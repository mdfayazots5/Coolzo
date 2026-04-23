namespace Coolzo.Contracts.Requests.Auth;

public sealed class ResetPasswordRequest
{
    public string? Token { get; init; }

    public string? Password { get; init; }

    public string? Phone { get; init; }

    public string? Otp { get; init; }

    public string? NewPassword { get; init; }
}
