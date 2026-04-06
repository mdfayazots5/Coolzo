using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class ServiceRequestStatusHistory : AuditableEntity
{
    public long ServiceRequestStatusHistoryId { get; set; }

    public long ServiceRequestId { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;

    public string Remarks { get; set; } = string.Empty;

    public DateTime StatusDateUtc { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}
