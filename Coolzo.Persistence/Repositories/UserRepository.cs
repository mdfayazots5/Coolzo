using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly CoolzoDbContext _dbContext;

    public UserRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public Task<int> CountAsync(
        string? searchTerm,
        bool? isActive,
        IReadOnlyCollection<long>? roleIds,
        IReadOnlyCollection<int>? branchIds,
        CancellationToken cancellationToken)
    {
        return BuildListQuery(searchTerm, isActive, roleIds, branchIds, includeRoles: false)
            .CountAsync(cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, long? excludedUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(
            user => !user.IsDeleted &&
                user.Email == email &&
                (!excludedUserId.HasValue || user.UserId != excludedUserId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByUserNameAsync(string userName, long? excludedUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(
            user => !user.IsDeleted &&
                user.UserName == userName &&
                (!excludedUserId.HasValue || user.UserId != excludedUserId.Value),
            cancellationToken);
    }

    public Task<User?> GetByIdWithRolesAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.UserId == userId && !user.IsDeleted, cancellationToken);
    }

    public Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == userId && !user.IsDeleted, cancellationToken);
    }

    public Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(
                user => !user.IsDeleted &&
                    (user.UserName == userNameOrEmail || user.Email == userNameOrEmail),
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        IReadOnlyCollection<long>? roleIds,
        IReadOnlyCollection<int>? branchIds,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = BuildListQuery(searchTerm, isActive, roleIds, branchIds, includeRoles: true);
        query = ApplyOrdering(query, sortBy, sortOrder);

        return await query
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<User> BuildListQuery(
        string? searchTerm,
        bool? isActive,
        IReadOnlyCollection<long>? roleIds,
        IReadOnlyCollection<int>? branchIds,
        bool includeRoles)
    {
        IQueryable<User> query = _dbContext.Users.AsNoTracking();

        if (includeRoles)
        {
            query = query
                .Include(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role);
        }

        query = query.Where(user => !user.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim();
            query = query.Where(
                user => user.FullName.Contains(normalizedSearchTerm) ||
                    user.UserName.Contains(normalizedSearchTerm) ||
                    user.Email.Contains(normalizedSearchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (roleIds is { Count: > 0 })
        {
            query = query.Where(
                user => user.UserRoles.Any(
                    userRole => !userRole.IsDeleted && roleIds.Contains(userRole.RoleId)));
        }

        if (branchIds is { Count: > 0 })
        {
            query = query.Where(user => branchIds.Contains(user.BranchId));
        }

        return query;
    }

    private static IQueryable<User> ApplyOrdering(IQueryable<User> query, string? sortBy, string? sortOrder)
    {
        var normalizedSortBy = sortBy?.Trim();
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return (normalizedSortBy, descending) switch
        {
            ("createdAt", true) => query.OrderByDescending(user => user.DateCreated).ThenBy(user => user.FullName),
            ("createdAt", false) => query.OrderBy(user => user.DateCreated).ThenBy(user => user.FullName),
            ("lastLogin", true) => query.OrderByDescending(user => user.LastLoginDateUtc).ThenBy(user => user.FullName),
            ("lastLogin", false) => query.OrderBy(user => user.LastLoginDateUtc).ThenBy(user => user.FullName),
            (_, true) => query.OrderByDescending(user => user.FullName).ThenByDescending(user => user.DateCreated),
            _ => query.OrderBy(user => user.FullName).ThenBy(user => user.DateCreated),
        };
    }
}
