using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class StockTransaction : AuditableEntity
{
    public long StockTransactionId { get; set; }

    public long ItemId { get; set; }

    public long? WarehouseId { get; set; }

    public long? TechnicianId { get; set; }

    public long? JobCardId { get; set; }

    public long? SupplierId { get; set; }

    public StockTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal Amount { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string TransactionGroupCode { get; set; } = string.Empty;

    public DateTime TransactionDateUtc { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public decimal BalanceAfterTransaction { get; set; }

    public Item? Item { get; set; }

    public Warehouse? Warehouse { get; set; }

    public Technician? Technician { get; set; }

    public JobCard? JobCard { get; set; }

    public Supplier? Supplier { get; set; }

    public ICollection<JobPartConsumption> JobPartConsumptions { get; set; } = new List<JobPartConsumption>();
}
