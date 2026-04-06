namespace Coolzo.Domain.Entities;

public sealed class ItemRate : AuditableEntity
{
    public long ItemRateId { get; set; }

    public long ItemId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public Item? Item { get; set; }
}
