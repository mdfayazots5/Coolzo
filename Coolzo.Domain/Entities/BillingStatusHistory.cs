using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class BillingStatusHistory : AuditableEntity
{
    public long BillingStatusHistoryId { get; set; }

    public long? QuotationHeaderId { get; set; }

    public long? InvoiceHeaderId { get; set; }

    public long? PaymentTransactionId { get; set; }

    public BillingEntityType EntityType { get; set; } = BillingEntityType.Quotation;

    public string StatusName { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime StatusDateUtc { get; set; }

    public QuotationHeader? QuotationHeader { get; set; }

    public InvoiceHeader? InvoiceHeader { get; set; }

    public PaymentTransaction? PaymentTransaction { get; set; }
}
