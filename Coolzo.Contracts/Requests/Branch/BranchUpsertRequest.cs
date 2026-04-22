namespace Coolzo.Contracts.Requests.Branch;

public sealed record BranchUpsertRequest(
    string Name,
    string City,
    string Address,
    long? ManagerId,
    IReadOnlyCollection<string>? Zones,
    bool IsActive);
