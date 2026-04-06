namespace Coolzo.Domain.Entities;

public sealed class SlotAvailability : AuditableEntity
{
    public long SlotAvailabilityId { get; set; }

    public long SlotConfigurationId { get; set; }

    public long ZoneId { get; set; }

    public DateOnly SlotDate { get; set; }

    public int AvailableCapacity { get; set; }

    public int ReservedCapacity { get; set; }

    public bool IsBlocked { get; set; }

    public SlotConfiguration? SlotConfiguration { get; set; }

    public Zone? Zone { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
