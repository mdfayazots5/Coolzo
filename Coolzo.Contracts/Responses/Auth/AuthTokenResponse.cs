namespace Coolzo.Contracts.Responses.Auth;

public sealed record AuthTokenResponse
(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    CurrentUserResponse CurrentUser
);
