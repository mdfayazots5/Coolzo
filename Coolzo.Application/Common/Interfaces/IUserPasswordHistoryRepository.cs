using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IUserPasswordHistoryRepository
{
    Task AddAsync(UserPasswordHistory passwordHistory, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserPasswordHistory>> ListByUserIdAsync(long userId, CancellationToken cancellationToken);
}
