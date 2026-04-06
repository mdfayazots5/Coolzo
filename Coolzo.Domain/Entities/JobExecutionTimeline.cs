using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class JobExecutionTimeline : AuditableEntity
{
    public long JobExecutionTimelineId { get; set; }

    public long JobCardId { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Assigned;

    public string EventType { get; set; } = string.Empty;

    public string EventTitle { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime EventDateUtc { get; set; }

    public JobCard? JobCard { get; set; }
}
