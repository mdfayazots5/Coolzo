namespace Coolzo.Contracts.Requests.Inventory;

public sealed record AssignStockToTechnicianRequest(
    long SourceWarehouseId,
    long ItemId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Remarks);
