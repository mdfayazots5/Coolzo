namespace Coolzo.Contracts.Requests.Inventory;

public sealed record TransferStockRequest(
    long SourceWarehouseId,
    long DestinationWarehouseId,
    long ItemId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Remarks);
