namespace Coolzo.Contracts.Requests.Customer;

public sealed record UpdateCustomerAddressRequest(
    long CustomerAddressId,
    string AddressLabel,
    string AddressLine1,
    string AddressLine2,
    string Landmark,
    string CityName,
    string Pincode,
    long? ZoneId,
    double? Latitude,
    double? Longitude,
    bool IsDefault,
    string? StateName,
    string? AddressType);
