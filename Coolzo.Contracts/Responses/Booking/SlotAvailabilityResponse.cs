namespace Coolzo.Contracts.Responses.Booking;

public sealed record SlotAvailabilityResponse(
    long SlotAvailabilityId,
    long ZoneId,
    DateOnly SlotDate,
    string SlotLabel,
    string StartTime,
    string EndTime,
    int AvailableCapacity,
    int ReservedCapacity,
    bool IsAvailable);
