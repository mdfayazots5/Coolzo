namespace Coolzo.Contracts.Requests.Booking;

public sealed record BookingSearchRequest
(
    string? BookingReference,
    string? CustomerMobile,
    DateOnly? BookingDate,
    long? ServiceId,
    int PageNumber = 1,
    int PageSize = 20
);
