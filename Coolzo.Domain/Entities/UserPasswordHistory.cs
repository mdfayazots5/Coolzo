using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class UserPasswordHistory : AuditableEntity
{
    public long UserPasswordHistoryId { get; set; }

    public long UserId { get; set; }

    public string PasswordValue { get; set; } = string.Empty;

    public CustomerPasswordMode PasswordStorageMode { get; set; } = CustomerPasswordMode.Hashed;

    public CustomerPasswordChangeSource ChangeSource { get; set; }

    public DateTime ChangedOnUtc { get; set; }

    public User? User { get; set; }
}
