namespace Coolzo.Contracts.Responses.Auth;

public sealed record RolePermissionSnapshotResponse(
    long RoleId,
    IReadOnlyCollection<long> PermissionIds,
    IReadOnlyDictionary<string, PermissionModuleActionsResponse> Modules,
    string DataScope,
    IReadOnlyCollection<string> Permissions,
    string RoleName,
    string DisplayName
);
