namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record SaveJobChecklistResponseItemRequest(
    long ServiceChecklistMasterId,
    bool? IsChecked,
    string? ResponseRemarks);
