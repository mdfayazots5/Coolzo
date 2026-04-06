namespace Coolzo.Domain.Entities;

public sealed class TechnicianVanStock : AuditableEntity
{
    public long TechnicianVanStockId { get; set; }

    public long TechnicianId { get; set; }

    public long ItemId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public DateTime? LastTransactionDateUtc { get; set; }

    public Technician? Technician { get; set; }

    public Item? Item { get; set; }
}
