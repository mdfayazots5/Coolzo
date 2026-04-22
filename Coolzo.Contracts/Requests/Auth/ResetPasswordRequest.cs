namespace Coolzo.Contracts.Requests.Auth;

public sealed record ResetPasswordRequest(string Token, string Password);
