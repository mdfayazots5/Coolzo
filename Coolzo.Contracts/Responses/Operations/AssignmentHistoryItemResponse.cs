namespace Coolzo.Contracts.Responses.Operations;

public sealed record AssignmentHistoryItemResponse(
    string ActionName,
    string? PreviousTechnicianName,
    string CurrentTechnicianName,
    string Remarks,
    DateTime ActionDateUtc);
