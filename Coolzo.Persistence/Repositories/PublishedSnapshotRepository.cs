using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class PublishedSnapshotRepository : IPublishedSnapshotRepository
{
    private readonly CoolzoDbContext _dbContext;

    public PublishedSnapshotRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(PublishedSnapshot publishedSnapshot, CancellationToken cancellationToken)
    {
        return _dbContext.PublishedSnapshots.AddAsync(publishedSnapshot, cancellationToken).AsTask();
    }

    public Task<PublishedSnapshot?> GetActiveAsync(CancellationToken cancellationToken)
    {
        return _dbContext.PublishedSnapshots
            .Where(entity => entity.IsActive && !entity.IsDeleted)
            .OrderByDescending(entity => entity.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<PublishedSnapshot?> GetByVersionAsync(int version, CancellationToken cancellationToken)
    {
        return _dbContext.PublishedSnapshots
            .FirstOrDefaultAsync(entity => entity.Version == version && !entity.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PublishedSnapshot>> GetActiveListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.PublishedSnapshots
            .Where(entity => entity.IsActive && !entity.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxVersionAsync(CancellationToken cancellationToken)
    {
        var hasAny = await _dbContext.PublishedSnapshots.AnyAsync(cancellationToken);

        if (!hasAny)
        {
            return 0;
        }

        return await _dbContext.PublishedSnapshots.MaxAsync(entity => entity.Version, cancellationToken);
    }
}
