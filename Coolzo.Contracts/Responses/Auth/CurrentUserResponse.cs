namespace Coolzo.Contracts.Responses.Auth;

public sealed record CurrentUserResponse
(
    long UserId,
    string UserName,
    string Email,
    string FullName,
    long? TechnicianId,
    long? HelperProfileId,
    int BranchId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    long? CustomerId,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc
);
