namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record DiagnosisLookupItemResponse(
    long Id,
    string Name,
    string Description);
