using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class PurchaseOrder : AuditableEntity
{
    public long PurchaseOrderId { get; set; }

    public string PONumber { get; set; } = string.Empty;

    public long SupplierId { get; set; }

    public DateTime OrderDateUtc { get; set; }

    public DateTime ExpectedDeliveryDateUtc { get; set; }

    public PurchaseOrderStatus CurrentStatus { get; set; } = PurchaseOrderStatus.Submitted;

    public DateTime? ReceivedAtUtc { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public Supplier? Supplier { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public sealed class PurchaseOrderItem : AuditableEntity
{
    public long PurchaseOrderItemId { get; set; }

    public long PurchaseOrderId { get; set; }

    public long ItemId { get; set; }

    public string PartCode { get; set; } = string.Empty;

    public string PartName { get; set; } = string.Empty;

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public bool DiscrepancyFlag { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    public Item? Item { get; set; }
}
