namespace Coolzo.Contracts.Responses.Booking;

public sealed record ZoneLookupResponse(long ZoneId, string ZoneName, string CityName, string Pincode);
