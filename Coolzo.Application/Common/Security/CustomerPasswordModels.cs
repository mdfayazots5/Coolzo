using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Security;

public sealed record CustomerPasswordPolicySnapshot(
    CustomerPasswordMode PasswordMode,
    CustomerPasswordGenerationMode PasswordGenerationMode,
    string? FixedDefaultPassword,
    int PasswordMinLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireNumber,
    bool RequireSpecialCharacter,
    bool ForcePasswordChangeOnFirstLogin,
    bool AllowAdminResetPassword,
    bool AllowSelfRegistrationPassword,
    bool AllowPlainTextModeOnlyInNonProduction,
    CustomerPasswordValidationMode PasswordValidationMode,
    bool PasswordHistoryEnabled,
    int PasswordExpiryDays);

public sealed record PreparedCustomerPassword(
    CustomerPasswordPolicySnapshot Policy,
    string RawPassword,
    string StoredPassword,
    CustomerPasswordMode PasswordStorageMode,
    CustomerPasswordChangeSource ChangeSource,
    bool PasswordGenerated,
    bool RequiresPasswordDelivery,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc);

public sealed record CustomerPasswordStateSnapshot(
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc,
    bool IsExpired);
