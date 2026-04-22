using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<int> CountAsync(
        string? searchTerm,
        bool? isActive,
        IReadOnlyCollection<long>? roleIds,
        IReadOnlyCollection<int>? branchIds,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, long? excludedUserId, CancellationToken cancellationToken);

    Task<bool> ExistsByUserNameAsync(string userName, long? excludedUserId, CancellationToken cancellationToken);

    Task<User?> GetByIdWithRolesAsync(long userId, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken);

    Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<User>> ListAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        IReadOnlyCollection<long>? roleIds,
        IReadOnlyCollection<int>? branchIds,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken);
}
