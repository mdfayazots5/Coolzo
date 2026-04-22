namespace Coolzo.Contracts.Responses.Booking;

public sealed record ZoneListItemResponse(long ZoneId, string ZoneCode, string ZoneName, string CityName);

public sealed record ZoneLookupResponse(long ZoneId, string ZoneName, string CityName, string Pincode);
