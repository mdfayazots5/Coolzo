namespace Coolzo.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public long RefreshTokenId { get; set; }

    public long UserId { get; set; }

    public long? UserSessionId { get; set; }

    public string TokenValue { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    public User? User { get; set; }

    public UserSession? UserSession { get; set; }
}
