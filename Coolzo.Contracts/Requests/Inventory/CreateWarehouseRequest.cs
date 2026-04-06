namespace Coolzo.Contracts.Requests.Inventory;

public sealed record CreateWarehouseRequest(
    string WarehouseCode,
    string WarehouseName,
    string? ContactPerson,
    string? MobileNumber,
    string? EmailAddress,
    string? AddressLine1,
    string? AddressLine2,
    string? Landmark,
    string? CityName,
    string? Pincode,
    bool IsActive);
