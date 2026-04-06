namespace Coolzo.Contracts.Responses.Booking;

public sealed record BookingStatusHistoryResponse(string Status, string Remarks, DateTime StatusDateUtc);
