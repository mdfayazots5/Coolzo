namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record UpdateTechnicianJobStatusRequest(
    string? Remarks,
    string? WorkSummary);
