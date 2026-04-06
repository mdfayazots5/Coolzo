namespace Coolzo.Contracts.Requests.Operations;

public sealed record AssignTechnicianRequest(
    long? TechnicianId,
    string? Remarks);
