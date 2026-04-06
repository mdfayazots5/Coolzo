namespace Coolzo.Contracts.Responses.Booking;

public sealed record BookingLineResponse(
    string ServiceName,
    string AcTypeName,
    string TonnageName,
    string BrandName,
    string ModelName,
    string IssueNotes,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
