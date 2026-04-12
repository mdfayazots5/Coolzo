namespace Coolzo.Contracts.Responses.Customer;

public sealed record CustomerAddressResponse(
    long CustomerAddressId,
    long CustomerId,
    string AddressLabel,
    string AddressLine1,
    string AddressLine2,
    string Landmark,
    string CityName,
    string StateName,
    string Pincode,
    string AddressType,
    long? ZoneId,
    double? Latitude,
    double? Longitude,
    bool IsDefault,
    bool IsActive,
    DateTime DateCreated,
    DateTime? LastUpdated);
