namespace Coolzo.Domain.Entities;

public sealed class TechnicianAvailability : AuditableEntity
{
    public long TechnicianAvailabilityId { get; set; }

    public long TechnicianId { get; set; }

    public DateOnly AvailableDate { get; set; }

    public int AvailableSlotCount { get; set; }

    public int BookedAssignmentCount { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string AvailabilityRemarks { get; set; } = string.Empty;

    public Technician? Technician { get; set; }
}
