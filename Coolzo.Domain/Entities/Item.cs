namespace Coolzo.Domain.Entities;

public sealed class Item : AuditableEntity
{
    public long ItemId { get; set; }

    public long ItemCategoryId { get; set; }

    public long UnitOfMeasureId { get; set; }

    public long? SupplierId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string ItemDescription { get; set; } = string.Empty;

    public decimal TaxPercentage { get; set; }

    public int WarrantyDays { get; set; }

    public decimal ReorderLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public ItemCategory? ItemCategory { get; set; }

    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public Supplier? Supplier { get; set; }

    public ICollection<ItemRate> Rates { get; set; } = new List<ItemRate>();

    public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();

    public ICollection<TechnicianVanStock> TechnicianVanStocks { get; set; } = new List<TechnicianVanStock>();

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public ICollection<JobPartConsumption> JobPartConsumptions { get; set; } = new List<JobPartConsumption>();
}
