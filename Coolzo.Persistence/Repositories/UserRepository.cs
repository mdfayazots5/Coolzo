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

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Users.CountAsync(user => !user.IsDeleted, cancellationToken);
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

    public async Task<IReadOnlyCollection<User>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .Where(user => !user.IsDeleted)
            .OrderBy(user => user.FullName)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }
}
