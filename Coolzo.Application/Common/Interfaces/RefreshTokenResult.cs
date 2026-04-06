namespace Coolzo.Application.Common.Interfaces;

public sealed record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);
