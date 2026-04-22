namespace Coolzo.Contracts.Requests.Role;

public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<long> PermissionIds);
