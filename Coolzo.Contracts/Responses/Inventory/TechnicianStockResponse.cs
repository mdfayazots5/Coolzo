namespace Coolzo.Contracts.Responses.Inventory;

public sealed record TechnicianStockResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    int TotalSkuCount,
    decimal TotalQuantityOnHand,
    IReadOnlyCollection<TechnicianStockItemResponse> Items);
