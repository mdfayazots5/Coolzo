namespace Coolzo.Contracts.Responses.CustomerAuth;

public sealed record CustomerPasswordOperationResponse(
    bool PasswordUpdated,
    bool PasswordGenerated,
    bool RequiresPasswordDelivery,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc);
