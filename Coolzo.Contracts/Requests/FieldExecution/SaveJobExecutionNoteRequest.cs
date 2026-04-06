namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record SaveJobExecutionNoteRequest(
    string NoteText,
    bool IsCustomerVisible);
