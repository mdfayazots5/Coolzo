namespace Coolzo.Domain.Entities;

public sealed class AssignmentLog : AuditableEntity
{
    public long AssignmentLogId { get; set; }

    public long ServiceRequestId { get; set; }

    public long? PreviousTechnicianId { get; set; }

    public long CurrentTechnicianId { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime ActionDateUtc { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public Technician? PreviousTechnician { get; set; }

    public Technician? CurrentTechnician { get; set; }
}
