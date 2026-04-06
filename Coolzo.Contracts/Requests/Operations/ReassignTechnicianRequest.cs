namespace Coolzo.Contracts.Requests.Operations;

public sealed record ReassignTechnicianRequest(
    long TechnicianId,
    string? Remarks);
