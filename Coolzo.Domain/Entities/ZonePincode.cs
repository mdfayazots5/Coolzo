namespace Coolzo.Domain.Entities;

public sealed class ZonePincode : AuditableEntity
{
    public long ZonePincodeId { get; set; }

    public long ZoneId { get; set; }

    public string Pincode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Zone? Zone { get; set; }
}
