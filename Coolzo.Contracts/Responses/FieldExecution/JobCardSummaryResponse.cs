namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobCardSummaryResponse(
    long? JobCardId,
    string? JobCardNumber,
    DateTime? WorkStartedDateUtc,
    DateTime? WorkInProgressDateUtc,
    DateTime? WorkCompletedDateUtc,
    DateTime? SubmittedForClosureDateUtc,
    string? CompletionSummary,
    int NoteCount,
    int AttachmentCount);
