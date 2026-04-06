namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobExecutionNoteResponse(
    long JobExecutionNoteId,
    string NoteText,
    bool IsCustomerVisible,
    string CreatedBy,
    DateTime NoteDateUtc);
