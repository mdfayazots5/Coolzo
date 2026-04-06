namespace Coolzo.Contracts.Responses.Amc;

public sealed record AmcVisitScheduleResponse(
    long AmcVisitScheduleId,
    int VisitNumber,
    DateOnly ScheduledDate,
    string CurrentStatus,
    long? ServiceRequestId,
    string? ServiceRequestNumber,
    DateTime? CompletedDateUtc,
    string VisitRemarks);
