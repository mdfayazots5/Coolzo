namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobChecklistItemResponse(
    long ServiceChecklistMasterId,
    string ChecklistTitle,
    string ChecklistDescription,
    bool IsMandatory,
    bool? IsChecked,
    string ResponseRemarks,
    DateTime? ResponseDateUtc);
