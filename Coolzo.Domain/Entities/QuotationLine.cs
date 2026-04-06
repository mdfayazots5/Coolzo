using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class QuotationLine : AuditableEntity
{
    public long QuotationLineId { get; set; }

    public long QuotationHeaderId { get; set; }

    public QuotationLineType LineType { get; set; } = QuotationLineType.Service;

    public string LineDescription { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineAmount { get; set; }

    public QuotationHeader? QuotationHeader { get; set; }
}
