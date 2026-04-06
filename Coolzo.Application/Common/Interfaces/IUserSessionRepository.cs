using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession userSession, CancellationToken cancellationToken);
}
