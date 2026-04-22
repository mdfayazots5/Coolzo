namespace Coolzo.Contracts.Responses.Auth;

public sealed record AuthPermissionSnapshotResponse(
    IReadOnlyDictionary<string, PermissionModuleActionsResponse> Modules,
    string DataScope,
    IReadOnlyCollection<string> Permissions
);
