using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class AmcVisitSchedule : AuditableEntity
{
    public long AmcVisitScheduleId { get; set; }

    public long CustomerAmcId { get; set; }

    public int VisitNumber { get; set; }

    public DateOnly ScheduledDate { get; set; }

    public AmcVisitStatus CurrentStatus { get; set; } = AmcVisitStatus.Scheduled;

    public long? ServiceRequestId { get; set; }

    public DateTime? CompletedDateUtc { get; set; }

    public string VisitRemarks { get; set; } = string.Empty;

    public CustomerAmc? CustomerAmc { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}
