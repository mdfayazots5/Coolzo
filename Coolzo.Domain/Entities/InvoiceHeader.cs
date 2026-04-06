using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class InvoiceHeader : AuditableEntity
{
    public long InvoiceHeaderId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public long QuotationHeaderId { get; set; }

    public long CustomerId { get; set; }

    public InvoicePaymentStatus CurrentStatus { get; set; } = InvoicePaymentStatus.Unpaid;

    public DateTime InvoiceDateUtc { get; set; }

    public decimal SubTotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxPercentage { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal GrandTotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public DateTime? LastPaymentDateUtc { get; set; }

    public QuotationHeader? QuotationHeader { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public ICollection<BillingStatusHistory> BillingStatusHistories { get; set; } = new List<BillingStatusHistory>();
}
