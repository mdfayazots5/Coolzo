namespace Coolzo.Contracts.Responses.User;

public sealed record UserResponse
(
    long UserId,
    string UserName,
    string Email,
    string FullName,
    bool IsActive,
    int BranchId,
    IReadOnlyCollection<long> RoleIds,
    IReadOnlyCollection<string> Roles,
    DateTime DateCreated,
    DateTime? LastLoginDateUtc,
    bool MustChangePassword
);
