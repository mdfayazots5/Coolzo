using Coolzo.Application.Common.Interfaces;
using Coolzo.Persistence.Context;

namespace Coolzo.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CoolzoDbContext _dbContext;

    public UnitOfWork(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
