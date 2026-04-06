namespace Coolzo.Contracts.Responses.Permission;

public sealed record PermissionResponse
(
    long PermissionId,
    string PermissionName,
    string DisplayName,
    string ModuleName,
    string ActionName,
    bool IsActive
);
