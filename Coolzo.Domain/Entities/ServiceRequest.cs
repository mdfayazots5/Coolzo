using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class ServiceRequest : AuditableEntity
{
    public long ServiceRequestId { get; set; }

    public string ServiceRequestNumber { get; set; } = string.Empty;

    public long BookingId { get; set; }

    public ServiceRequestStatus CurrentStatus { get; set; } = ServiceRequestStatus.New;

    public DateTime ServiceRequestDateUtc { get; set; }

    public Booking? Booking { get; set; }

    public ICollection<ServiceRequestAssignment> Assignments { get; set; } = new List<ServiceRequestAssignment>();

    public ICollection<ServiceRequestStatusHistory> StatusHistories { get; set; } = new List<ServiceRequestStatusHistory>();

    public ICollection<AssignmentLog> AssignmentLogs { get; set; } = new List<AssignmentLog>();

    public JobCard? JobCard { get; set; }
}
