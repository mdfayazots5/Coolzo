namespace Coolzo.Domain.Entities;

public sealed class SlotConfiguration : AuditableEntity
{
    public long SlotConfigurationId { get; set; }

    public long ZoneId { get; set; }

    public string SlotLabel { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int MaxBookingCount { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public Zone? Zone { get; set; }

    public ICollection<SlotAvailability> SlotAvailabilities { get; set; } = new List<SlotAvailability>();
}
