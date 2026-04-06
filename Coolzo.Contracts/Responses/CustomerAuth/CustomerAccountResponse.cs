namespace Coolzo.Contracts.Responses.CustomerAuth;

public sealed record CustomerAccountResponse(
    long CustomerId,
    long UserId,
    string CustomerName,
    string MobileNumber,
    string EmailAddress,
    bool PasswordGenerated,
    bool RequiresPasswordDelivery,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc);
