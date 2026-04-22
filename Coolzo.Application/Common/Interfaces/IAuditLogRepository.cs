using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AuditLog>> ListRecentUserActivityAsync(
        long userId,
        string userName,
        int take,
        CancellationToken cancellationToken);
}
