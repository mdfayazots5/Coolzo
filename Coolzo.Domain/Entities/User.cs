using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class User : AuditableEntity
{
    public long UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public CustomerPasswordMode PasswordStorageMode { get; set; } = CustomerPasswordMode.Hashed;

    public bool MustChangePassword { get; set; }

    public DateTime? PasswordLastChangedOnUtc { get; set; }

    public DateTime? PasswordExpiryOnUtc { get; set; }

    public string? PasswordUpdatedBy { get; set; }

    public bool IsTemporaryPassword { get; set; }

    public DateTime? LastPasswordResetOnUtc { get; set; }

    public CustomerPasswordChangeSource? PasswordResetSource { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginDateUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public ICollection<UserPasswordHistory> PasswordHistories { get; set; } = new List<UserPasswordHistory>();
}
