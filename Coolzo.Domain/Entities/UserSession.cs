namespace Coolzo.Domain.Entities;

public sealed class UserSession : AuditableEntity
{
    public long UserSessionId { get; set; }

    public long UserId { get; set; }

    public string AccessTokenJti { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string? DeviceName { get; set; }

    public string? PlatformName { get; set; }

    public string? SessionIpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public User? User { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
