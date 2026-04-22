using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<int> RevokeActiveByUserIdAsync(long userId, DateTime revokedAtUtc, string revokedBy, CancellationToken cancellationToken);
}
