namespace Coolzo.Contracts.Responses.Inventory;

public sealed record WarehouseResponse(
    long WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string ContactPerson,
    string MobileNumber,
    string EmailAddress,
    string CityName,
    string AddressSummary,
    bool IsActive,
    int StockItemCount,
    decimal TotalQuantityOnHand);
