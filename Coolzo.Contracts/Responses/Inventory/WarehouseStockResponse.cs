namespace Coolzo.Contracts.Responses.Inventory;

public sealed record WarehouseStockResponse(
    long WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    int TotalSkuCount,
    decimal TotalQuantityOnHand,
    IReadOnlyCollection<WarehouseStockItemResponse> Items);
