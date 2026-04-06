namespace Coolzo.Application.Common.Interfaces;

public sealed record AccessTokenResult(string Token, string TokenId, DateTime ExpiresAtUtc);
