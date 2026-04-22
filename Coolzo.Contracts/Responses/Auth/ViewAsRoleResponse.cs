namespace Coolzo.Contracts.Responses.Auth;

public sealed record ViewAsRoleResponse(
    long RoleId,
    string RoleName,
    string DisplayName,
    IReadOnlyDictionary<string, PermissionModuleActionsResponse> Modules,
    string DataScope,
    IReadOnlyCollection<string> Permissions,
    DateTimeOffset IssuedAtUtc);
