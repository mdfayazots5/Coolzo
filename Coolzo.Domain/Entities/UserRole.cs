namespace Coolzo.Domain.Entities;

public sealed class UserRole : AuditableEntity
{
    public long UserRoleId { get; set; }

    public long UserId { get; set; }

    public long RoleId { get; set; }

    public User? User { get; set; }

    public Role? Role { get; set; }
}
