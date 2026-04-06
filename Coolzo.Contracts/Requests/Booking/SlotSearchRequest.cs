namespace Coolzo.Contracts.Requests.Booking;

public sealed record SlotSearchRequest(long ZoneId, DateOnly SlotDate);
