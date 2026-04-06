namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobExecutionTimelineItemResponse(
    string EventType,
    string EventTitle,
    string Status,
    string Remarks,
    DateTime EventDateUtc);
