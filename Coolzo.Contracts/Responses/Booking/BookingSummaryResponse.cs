namespace Coolzo.Contracts.Responses.Booking;

public sealed record BookingSummaryResponse(
    long BookingId,
    string BookingReference,
    string Status,
    string ServiceName,
    string CustomerName,
    string MobileNumber,
    DateOnly SlotDate,
    string SlotLabel,
    string AddressSummary,
    decimal EstimatedPrice);
