using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class WarrantyClaim : AuditableEntity
{
    public long WarrantyClaimId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public long CustomerId { get; set; }

    public long JobCardId { get; set; }

    public long? WarrantyRuleId { get; set; }

    public WarrantyClaimStatus CurrentStatus { get; set; } = WarrantyClaimStatus.Submitted;

    public DateTime ClaimDateUtc { get; set; }

    public DateTime CoverageStartDateUtc { get; set; }

    public DateTime CoverageEndDateUtc { get; set; }

    public bool IsEligible { get; set; }

    public string ClaimRemarks { get; set; } = string.Empty;

    public InvoiceHeader? InvoiceHeader { get; set; }

    public Customer? Customer { get; set; }

    public JobCard? JobCard { get; set; }

    public WarrantyRule? WarrantyRule { get; set; }

    public RevisitRequest? RevisitRequest { get; set; }
}
