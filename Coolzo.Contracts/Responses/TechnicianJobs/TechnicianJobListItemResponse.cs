namespace Coolzo.Contracts.Responses.TechnicianJobs;

public sealed record TechnicianJobListItemResponse(
    long ServiceRequestId,
    long? JobCardId,
    string ServiceRequestNumber,
    string? JobCardNumber,
    string LifecycleType,
    string LifecycleLabel,
    string BookingReference,
    string CustomerName,
    string MobileNumber,
    string AddressSummary,
    string ServiceName,
    string CurrentStatus,
    DateOnly SlotDate,
    string SlotLabel);
