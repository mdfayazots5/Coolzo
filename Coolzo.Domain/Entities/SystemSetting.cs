namespace Coolzo.Domain.Entities;

public sealed class SystemSetting : AuditableEntity
{
    public long SystemSettingId { get; set; }

    public string SettingKey { get; set; } = string.Empty;

    public string SettingValue { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }
}
