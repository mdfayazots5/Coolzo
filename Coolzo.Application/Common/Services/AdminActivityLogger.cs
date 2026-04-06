using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class AdminActivityLogger
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;

    public AdminActivityLogger(
        IAuditLogRepository auditLogRepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _auditLogRepository = auditLogRepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public Task WriteAsync(
        string actionName,
        string entityName,
        string entityId,
        string? newValues,
        CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = entityName,
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues ?? string.Empty,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
