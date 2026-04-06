namespace Coolzo.Domain.Entities;

public sealed class AuditLog : AuditableEntity
{
    public long AuditLogId { get; set; }

    public long? UserId { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string TraceId { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }
}
