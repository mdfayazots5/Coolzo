namespace Coolzo.Contracts.Requests.Role;

public sealed record UpdateRoleRequest
(
    long RoleId,
    string DisplayName,
    string Description,
    bool IsActive,
    IReadOnlyCollection<long> PermissionIds
);
