namespace Coolzo.Contracts.Responses.Inventory;

public sealed record WarehouseStockItemResponse(
    long WarehouseStockId,
    long ItemId,
    string ItemCode,
    string ItemName,
    string CategoryName,
    string UnitOfMeasureCode,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    bool IsLowStock,
    DateTime? LastTransactionDateUtc);
