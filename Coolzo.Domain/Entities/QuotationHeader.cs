using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class QuotationHeader : AuditableEntity
{
    public long QuotationHeaderId { get; set; }

    public string QuotationNumber { get; set; } = string.Empty;

    public long JobCardId { get; set; }

    public long CustomerId { get; set; }

    public QuotationStatus CurrentStatus { get; set; } = QuotationStatus.PendingCustomerApproval;

    public DateTime QuotationDateUtc { get; set; }

    public decimal SubTotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxPercentage { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal GrandTotalAmount { get; set; }

    public string CustomerDecisionRemarks { get; set; } = string.Empty;

    public DateTime? ApprovedDateUtc { get; set; }

    public DateTime? RejectedDateUtc { get; set; }

    public JobCard? JobCard { get; set; }

    public Customer? Customer { get; set; }

    public InvoiceHeader? InvoiceHeader { get; set; }

    public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();

    public ICollection<BillingStatusHistory> BillingStatusHistories { get; set; } = new List<BillingStatusHistory>();
}
