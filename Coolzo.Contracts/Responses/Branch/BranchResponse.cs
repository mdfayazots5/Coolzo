namespace Coolzo.Contracts.Responses.Branch;

public sealed record BranchResponse(
    int BranchId,
    string Name,
    string City,
    string Address,
    long? ManagerId,
    string? ManagerName,
    IReadOnlyCollection<string> Zones,
    bool IsActive,
    int TechnicianCount,
    int ServiceRequestCount);
