namespace Coolzo.Domain.Entities;

public sealed class RolePermission : AuditableEntity
{
    public long RolePermissionId { get; set; }

    public long RoleId { get; set; }

    public long PermissionId { get; set; }

    public Role? Role { get; set; }

    public Permission? Permission { get; set; }
}
