using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly CoolzoDbContext _dbContext;

    public AuditLogRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        return _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<AuditLog>> ListRecentUserActivityAsync(
        long userId,
        string userName,
        int take,
        CancellationToken cancellationToken)
    {
        var userIdValue = userId.ToString();

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(
                auditLog => !auditLog.IsDeleted &&
                    auditLog.EntityName == "User" &&
                    (auditLog.EntityId == userIdValue || auditLog.EntityId == userName))
            .OrderByDescending(auditLog => auditLog.DateCreated)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }
}
