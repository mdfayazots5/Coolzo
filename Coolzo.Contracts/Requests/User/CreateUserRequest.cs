namespace Coolzo.Contracts.Requests.User;

public sealed record CreateUserRequest
(
    string UserName,
    string Email,
    string FullName,
    string Password,
    bool IsActive,
    IReadOnlyCollection<long> RoleIds,
    int? BranchId
);
