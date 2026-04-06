namespace Coolzo.Domain.Entities;

public sealed class OtpVerification : AuditableEntity
{
    public long OtpVerificationId { get; set; }

    public long UserId { get; set; }

    public string OtpCode { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsConsumed { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }
}
