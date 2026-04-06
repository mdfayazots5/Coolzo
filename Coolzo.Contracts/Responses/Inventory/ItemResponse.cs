namespace Coolzo.Contracts.Responses.Inventory;

public sealed record ItemResponse(
    long ItemId,
    string ItemCode,
    string ItemName,
    string CategoryCode,
    string CategoryName,
    string UnitOfMeasureCode,
    string UnitOfMeasureName,
    long? SupplierId,
    string? SupplierCode,
    string? SupplierName,
    string ItemDescription,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal TaxPercentage,
    int WarrantyDays,
    decimal ReorderLevel,
    bool IsActive);
