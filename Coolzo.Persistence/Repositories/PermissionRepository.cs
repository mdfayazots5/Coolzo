using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly CoolzoDbContext _dbContext;

    public PermissionRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Permissions.CountAsync(permission => !permission.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Permission>> GetByIdsAsync(IReadOnlyCollection<long> permissionIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Permissions
            .Where(permission => !permission.IsDeleted && permissionIds.Contains(permission.PermissionId))
            .OrderBy(permission => permission.DisplayName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Permission>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await _dbContext.Permissions
            .AsNoTracking()
            .Where(permission => !permission.IsDeleted)
            .OrderBy(permission => permission.ModuleName)
            .ThenBy(permission => permission.ActionName)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }
}
