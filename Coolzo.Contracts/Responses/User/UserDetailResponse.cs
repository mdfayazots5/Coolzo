namespace Coolzo.Contracts.Responses.User;

public sealed record UserDetailResponse
(
    long UserId,
    string UserName,
    string Email,
    string FullName,
    bool IsActive,
    int BranchId,
    IReadOnlyCollection<long> RoleIds,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    DateTime DateCreated,
    DateTime? LastUpdated,
    DateTime? LastLoginDateUtc,
    bool MustChangePassword,
    bool IsTemporaryPassword,
    DateTime? PasswordExpiryOnUtc,
    IReadOnlyCollection<UserActivityResponse> RecentActivity
);
