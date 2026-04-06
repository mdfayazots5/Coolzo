namespace Coolzo.Domain.Entities;

public sealed class JobPartConsumption : AuditableEntity
{
    public long JobPartConsumptionId { get; set; }

    public long JobCardId { get; set; }

    public long TechnicianId { get; set; }

    public long ItemId { get; set; }

    public long StockTransactionId { get; set; }

    public decimal QuantityUsed { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineAmount { get; set; }

    public DateTime ConsumedDateUtc { get; set; }

    public string ConsumptionRemarks { get; set; } = string.Empty;

    public JobCard? JobCard { get; set; }

    public Technician? Technician { get; set; }

    public Item? Item { get; set; }

    public StockTransaction? StockTransaction { get; set; }
}
