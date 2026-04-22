using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly CoolzoDbContext _dbContext;

    public RoleRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken)
    {
        return _dbContext.Roles.AddAsync(role, cancellationToken).AsTask();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Roles.CountAsync(role => !role.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetByIdsAsync(IReadOnlyCollection<long> roleIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Roles
            .Where(role => !role.IsDeleted && roleIds.Contains(role.RoleId))
            .OrderBy(role => role.DisplayName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Role?> GetByIdWithPermissionsAsync(long roleId, CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.RoleId == roleId && !role.IsDeleted, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.RoleName == roleName && !role.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await _dbContext.Roles
            .AsNoTracking()
            .Include(role => role.UserRoles)
                .ThenInclude(userRole => userRole.User)
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Where(role => !role.IsDeleted)
            .OrderBy(role => role.DisplayName)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }
}
