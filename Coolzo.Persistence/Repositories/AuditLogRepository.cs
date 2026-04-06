using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;

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
}
