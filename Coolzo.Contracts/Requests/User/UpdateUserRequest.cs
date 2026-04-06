namespace Coolzo.Contracts.Requests.User;

public sealed record UpdateUserRequest
(
    long UserId,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<long> RoleIds
);
