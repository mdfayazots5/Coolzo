using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.GapPhaseA;
using MediatR;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Features.GapPhaseA.SystemHealth;

public sealed record GetSystemHealthSnapshotQuery() : IRequest<SystemHealthResponse>;

public sealed class GetSystemHealthSnapshotQueryHandler : IRequestHandler<GetSystemHealthSnapshotQuery, SystemHealthResponse>
{
    private static readonly IReadOnlyCollection<string> RequiredTriggerCodes =
    [
        "lead.received",
        "customer.absent",
        "estimate.rejected",
        "warranty.expiry",
        "payment.reminder",
        "escalation.alert"
    ];

    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IGapPhaseARepository _repository;

    public GetSystemHealthSnapshotQueryHandler(
        IGapPhaseARepository repository,
        IAdminConfigurationRepository adminConfigurationRepository,
        ICurrentDateTime currentDateTime)
    {
        _repository = repository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<SystemHealthResponse> Handle(GetSystemHealthSnapshotQuery request, CancellationToken cancellationToken)
    {
        var openAlertCount = await _repository.CountOpenAlertsAsync(cancellationToken);
        var pendingOfflineSyncCount = await _repository.CountPendingOfflineSyncItemsAsync(cancellationToken);
        var pendingWebhookRetryCount = (await _repository.GetPendingWebhookAttemptsAsync(_currentDateTime.UtcNow, cancellationToken)).Count;
        var enabledFeatureFlagCount = (await _repository.GetFeatureFlagsAsync(true, cancellationToken)).Count;
        var triggerCodes = (await _adminConfigurationRepository.SearchNotificationTriggersAsync(null, true, cancellationToken))
            .Select(trigger => trigger.TriggerCode)
            .Where(RequiredTriggerCodes.Contains)
            .OrderBy(trigger => trigger)
            .ToArray();

        var status = openAlertCount > 0 || pendingWebhookRetryCount > 0
            ? "Degraded"
            : "Healthy";

        return new SystemHealthResponse(
            status,
            openAlertCount,
            pendingOfflineSyncCount,
            pendingWebhookRetryCount,
            enabledFeatureFlagCount,
            triggerCodes);
    }
}
