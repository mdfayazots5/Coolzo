namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobChecklistSummaryResponse(
    int TotalItems,
    int RespondedItems,
    int MandatoryItems,
    int MandatoryRespondedItems);
