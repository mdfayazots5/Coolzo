namespace Coolzo.Domain.Entities;

public sealed class Warehouse : AuditableEntity
{
    public long WarehouseId { get; set; }

    public string WarehouseCode { get; set; } = string.Empty;

    public string WarehouseName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string Landmark { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
