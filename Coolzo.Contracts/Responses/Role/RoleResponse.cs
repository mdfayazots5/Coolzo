namespace Coolzo.Contracts.Responses.Role;

public sealed record RoleResponse
(
    long RoleId,
    string RoleName,
    string DisplayName,
    string Description,
    bool IsActive,
    int UserCount,
    IReadOnlyCollection<long> PermissionIds,
    IReadOnlyCollection<string> Permissions
);
