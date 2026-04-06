namespace Coolzo.Domain.Entities;

public sealed class Supplier : AuditableEntity
{
    public long SupplierId { get; set; }

    public string SupplierCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Item> Items { get; set; } = new List<Item>();

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
