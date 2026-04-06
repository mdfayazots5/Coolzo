using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class RevisitRequest : AuditableEntity
{
    public long RevisitRequestId { get; set; }

    public long BookingId { get; set; }

    public long CustomerId { get; set; }

    public long OriginalServiceRequestId { get; set; }

    public long OriginalJobCardId { get; set; }

    public long? ServiceRequestId { get; set; }

    public long? CustomerAmcId { get; set; }

    public long? WarrantyClaimId { get; set; }

    public RevisitType RevisitType { get; set; } = RevisitType.Paid;

    public RevisitStatus CurrentStatus { get; set; } = RevisitStatus.Requested;

    public DateTime RequestedDateUtc { get; set; }

    public DateTime? PreferredVisitDateUtc { get; set; }

    public string IssueSummary { get; set; } = string.Empty;

    public string RequestRemarks { get; set; } = string.Empty;

    public decimal ChargeAmount { get; set; }

    public Booking? Booking { get; set; }

    public Customer? Customer { get; set; }

    public ServiceRequest? OriginalServiceRequest { get; set; }

    public JobCard? OriginalJobCard { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public CustomerAmc? CustomerAmc { get; set; }

    public WarrantyClaim? WarrantyClaim { get; set; }
}
