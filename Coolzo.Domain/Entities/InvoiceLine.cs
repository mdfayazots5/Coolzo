using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class InvoiceLine : AuditableEntity
{
    public long InvoiceLineId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public long? QuotationLineId { get; set; }

    public QuotationLineType LineType { get; set; } = QuotationLineType.Service;

    public string LineDescription { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineAmount { get; set; }

    public InvoiceHeader? InvoiceHeader { get; set; }

    public QuotationLine? QuotationLine { get; set; }
}
