using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class UserPasswordHistoryRepository : IUserPasswordHistoryRepository
{
    private readonly CoolzoDbContext _dbContext;

    public UserPasswordHistoryRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(UserPasswordHistory passwordHistory, CancellationToken cancellationToken)
    {
        return _dbContext.UserPasswordHistories.AddAsync(passwordHistory, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<UserPasswordHistory>> ListByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserPasswordHistories
            .AsNoTracking()
            .Where(entity => entity.UserId == userId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.ChangedOnUtc)
            .ToArrayAsync(cancellationToken);
    }
}
