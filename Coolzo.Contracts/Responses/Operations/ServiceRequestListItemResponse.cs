namespace Coolzo.Contracts.Responses.Operations;

public sealed record ServiceRequestListItemResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    long BookingId,
    string BookingReference,
    string CustomerName,
    string ServiceName,
    string CurrentStatus,
    string? TechnicianName,
    DateOnly SlotDate,
    string SlotLabel,
    DateTime ServiceRequestDateUtc);
