namespace Coolzo.Domain.Entities;

public sealed class WarehouseStock : AuditableEntity
{
    public long WarehouseStockId { get; set; }

    public long WarehouseId { get; set; }

    public long ItemId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public DateTime? LastTransactionDateUtc { get; set; }

    public Warehouse? Warehouse { get; set; }

    public Item? Item { get; set; }
}
