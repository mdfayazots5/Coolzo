namespace Coolzo.Domain.Entities;

public sealed class Permission : AuditableEntity
{
    public long PermissionId { get; set; }

    public string PermissionName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public string ActionName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
