namespace Coolzo.Contracts.Requests.Role;

public sealed record CreateRoleRequest
(
    string RoleName,
    string DisplayName,
    string Description,
    bool IsActive,
    IReadOnlyCollection<long> PermissionIds
);
