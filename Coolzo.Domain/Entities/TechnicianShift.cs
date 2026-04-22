namespace Coolzo.Domain.Entities;

public sealed class TechnicianShift : AuditableEntity
{
    public long TechnicianShiftId { get; set; }

    public long TechnicianId { get; set; }

    public int DayOfWeekNumber { get; set; }

    public TimeOnly? ShiftStartTimeLocal { get; set; }

    public TimeOnly? ShiftEndTimeLocal { get; set; }

    public TimeOnly? BreakStartTimeLocal { get; set; }

    public TimeOnly? BreakEndTimeLocal { get; set; }

    public bool IsOffDuty { get; set; }

    public Technician? Technician { get; set; }
}
