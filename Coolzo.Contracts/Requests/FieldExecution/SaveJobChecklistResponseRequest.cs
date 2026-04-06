namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record SaveJobChecklistResponseRequest(
    IReadOnlyCollection<SaveJobChecklistResponseItemRequest> Items);
