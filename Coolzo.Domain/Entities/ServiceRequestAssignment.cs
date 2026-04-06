namespace Coolzo.Domain.Entities;

public sealed class ServiceRequestAssignment : AuditableEntity
{
    public long ServiceRequestAssignmentId { get; set; }

    public long ServiceRequestId { get; set; }

    public long TechnicianId { get; set; }

    public bool IsAutoAssigned { get; set; }

    public bool IsActiveAssignment { get; set; } = true;

    public DateTime AssignedDateUtc { get; set; }

    public DateTime? UnassignedDateUtc { get; set; }

    public string AssignmentRemarks { get; set; } = string.Empty;

    public string UnassignmentRemarks { get; set; } = string.Empty;

    public ServiceRequest? ServiceRequest { get; set; }

    public Technician? Technician { get; set; }
}
