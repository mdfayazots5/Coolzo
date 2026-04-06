namespace Coolzo.Contracts.Requests.Auth;

public sealed record LoginRequest(string UserNameOrEmail, string Password);
