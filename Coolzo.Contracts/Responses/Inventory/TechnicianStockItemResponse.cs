namespace Coolzo.Contracts.Responses.Inventory;

public sealed record TechnicianStockItemResponse(
    long TechnicianVanStockId,
    long ItemId,
    string ItemCode,
    string ItemName,
    string CategoryName,
    string UnitOfMeasureCode,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    bool IsLowStock,
    DateTime? LastTransactionDateUtc);
