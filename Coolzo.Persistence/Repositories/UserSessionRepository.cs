using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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

    public Task<int> DeactivateByUserIdAsync(long userId, DateTime updatedAtUtc, string updatedBy, CancellationToken cancellationToken)
    {
        return _dbContext.UserSessions
            .Where(userSession => userSession.UserId == userId &&
                userSession.IsActive &&
                !userSession.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(userSession => userSession.IsActive, false)
                    .SetProperty(userSession => userSession.UpdatedBy, updatedBy)
                    .SetProperty(userSession => userSession.LastUpdated, updatedAtUtc),
                cancellationToken);
    }
}
