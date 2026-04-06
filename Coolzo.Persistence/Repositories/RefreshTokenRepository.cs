using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CoolzoDbContext _dbContext;

    public RefreshTokenRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.ExpiresAtUtc <= utcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .Include(token => token.UserSession)
            .Include(token => token.User)
                .ThenInclude(user => user!.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                        .ThenInclude(role => role!.RolePermissions)
                            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(token => token.TokenValue == refreshToken && !token.IsDeleted, cancellationToken);
    }
}
