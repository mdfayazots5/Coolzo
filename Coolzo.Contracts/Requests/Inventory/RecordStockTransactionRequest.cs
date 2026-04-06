namespace Coolzo.Contracts.Requests.Inventory;

public sealed record RecordStockTransactionRequest(
    long WarehouseId,
    long ItemId,
    string TransactionType,
    decimal Quantity,
    decimal UnitCost,
    long? SupplierId,
    string? ReferenceNumber,
    string? Remarks);
