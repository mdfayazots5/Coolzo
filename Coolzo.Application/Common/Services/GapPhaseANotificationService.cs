using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class GapPhaseANotificationService
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public GapPhaseANotificationService(
        IGapPhaseARepository gapPhaseARepository,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public Task<SystemAlert> RaiseAlertAsync(
        string alertCode,
        string triggerCode,
        string alertType,
        string relatedEntityName,
        string relatedEntityId,
        SystemAlertSeverity severity,
        string message,
        DateTime? slaDueDateUtc,
        int escalationLevel,
        string notificationChain,
        CancellationToken cancellationToken)
    {
        var alert = new SystemAlert
        {
            AlertCode = alertCode,
            TriggerCode = triggerCode,
            AlertType = alertType,
            RelatedEntityName = relatedEntityName,
            RelatedEntityId = relatedEntityId,
            Severity = severity,
            AlertStatus = SystemAlertStatus.Open,
            AlertMessage = message,
            SlaDueDateUtc = slaDueDateUtc,
            EscalationLevel = escalationLevel,
            NotificationChain = notificationChain,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        return AddAndReturnAsync(alert, cancellationToken);
    }

    private async Task<SystemAlert> AddAndReturnAsync(SystemAlert alert, CancellationToken cancellationToken)
    {
        await _gapPhaseARepository.AddSystemAlertAsync(alert, cancellationToken);

        return alert;
    }
}
