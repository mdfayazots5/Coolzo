using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;

namespace Coolzo.Persistence.Repositories;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly CoolzoDbContext _dbContext;

    public UserSessionRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(UserSession userSession, CancellationToken cancellationToken)
    {
        return _dbContext.UserSessions.AddAsync(userSession, cancellationToken).AsTask();
    }
}
