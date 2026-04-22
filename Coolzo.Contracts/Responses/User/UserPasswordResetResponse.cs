namespace Coolzo.Contracts.Responses.User;

public sealed record UserPasswordResetResponse
(
    bool PasswordUpdated,
    bool PasswordGenerated,
    bool RequiresPasswordDelivery,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc,
    string? TemporaryPassword
);
