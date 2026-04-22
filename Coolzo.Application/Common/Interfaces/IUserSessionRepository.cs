using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession userSession, CancellationToken cancellationToken);

    Task<int> DeactivateByUserIdAsync(long userId, DateTime updatedAtUtc, string updatedBy, CancellationToken cancellationToken);
}
