namespace Coolzo.Contracts.Requests.Inventory;

public sealed record CreateItemRequest(
    string CategoryCode,
    string CategoryName,
    string UnitOfMeasureCode,
    string UnitOfMeasureName,
    string? SupplierCode,
    string? SupplierName,
    string ItemCode,
    string ItemName,
    string? ItemDescription,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal TaxPercentage,
    int WarrantyDays,
    decimal ReorderLevel,
    bool IsActive);
