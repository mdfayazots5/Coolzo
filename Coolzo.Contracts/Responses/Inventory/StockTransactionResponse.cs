namespace Coolzo.Contracts.Responses.Inventory;

public sealed record StockTransactionResponse(
    long StockTransactionId,
    long ItemId,
    string ItemCode,
    string ItemName,
    string TransactionType,
    long? WarehouseId,
    string? WarehouseName,
    long? TechnicianId,
    string? TechnicianName,
    long? JobCardId,
    string? JobCardNumber,
    decimal Quantity,
    decimal UnitCost,
    decimal Amount,
    decimal BalanceAfterTransaction,
    string ReferenceNumber,
    string TransactionGroupCode,
    DateTime TransactionDateUtc,
    string Remarks,
    string? SupplierName,
    string CreatedBy);
