using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;

namespace Coolzo.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider, ICurrentDateTime currentDateTime, IAppLogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var gapPhaseARepository = scope.ServiceProvider.GetRequiredService<IGapPhaseARepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var deletedCount = await refreshTokenRepository.DeleteExpiredAsync(_currentDateTime.UtcNow, stoppingToken);
            var webhookRetries = await ProcessWebhookRetriesAsync(gapPhaseARepository, _currentDateTime.UtcNow, stoppingToken);
            var offlineRetries = await ProcessOfflineSyncRetriesAsync(gapPhaseARepository, _currentDateTime.UtcNow, stoppingToken);
            var escalatedAlerts = await ProcessDueAlertsAsync(gapPhaseARepository, _currentDateTime.UtcNow, stoppingToken);

            await unitOfWork.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "Worker maintenance completed at {Time}. Deleted refresh tokens: {DeletedCount}. Webhook retries: {WebhookRetries}. Offline sync retries: {OfflineRetries}. Escalated alerts: {EscalatedAlerts}",
                _currentDateTime.UtcNow,
                deletedCount,
                webhookRetries,
                offlineRetries,
                escalatedAlerts);

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private static async Task<int> ProcessWebhookRetriesAsync(IGapPhaseARepository repository, DateTime utcNow, CancellationToken cancellationToken)
    {
        var attempts = await repository.GetPendingWebhookAttemptsAsync(utcNow, cancellationToken);

        foreach (var attempt in attempts)
        {
            attempt.RetryCount += 1;
            attempt.LastAttemptDateUtc = utcNow;
            attempt.UpdatedBy = "GapPhaseAWorker";
            attempt.LastUpdated = utcNow;

            if (attempt.RetryCount >= 3)
            {
                attempt.AttemptStatus = PaymentWebhookAttemptStatus.Failed;
                attempt.FailureReason = string.IsNullOrWhiteSpace(attempt.FailureReason)
                    ? "Webhook retry limit exceeded."
                    : attempt.FailureReason;
                attempt.NextRetryDateUtc = null;
            }
            else
            {
                attempt.AttemptStatus = PaymentWebhookAttemptStatus.RetryPending;
                attempt.FailureReason = string.IsNullOrWhiteSpace(attempt.FailureReason)
                    ? "Webhook retry rescheduled by worker."
                    : attempt.FailureReason;
                attempt.NextRetryDateUtc = utcNow.AddMinutes(15);
            }
        }

        return attempts.Count;
    }

    private static async Task<int> ProcessOfflineSyncRetriesAsync(IGapPhaseARepository repository, DateTime utcNow, CancellationToken cancellationToken)
    {
        var items = await repository.GetPendingOfflineSyncQueueItemsAsync(utcNow, cancellationToken);

        foreach (var item in items)
        {
            item.RetryCount += 1;
            item.LastAttemptDateUtc = utcNow;
            item.UpdatedBy = "GapPhaseAWorker";
            item.LastUpdated = utcNow;

            if (item.RetryCount >= 3)
            {
                item.SyncStatus = OfflineSyncStatus.Conflict;
                item.FailureReason = string.IsNullOrWhiteSpace(item.FailureReason)
                    ? "Offline sync retry limit exceeded."
                    : item.FailureReason;
                item.NextRetryDateUtc = null;
            }
            else
            {
                item.SyncStatus = OfflineSyncStatus.Failed;
                item.FailureReason = string.IsNullOrWhiteSpace(item.FailureReason)
                    ? "Offline sync retry rescheduled by worker."
                    : item.FailureReason;
                item.NextRetryDateUtc = utcNow.AddMinutes(10);
            }
        }

        return items.Count;
    }

    private static async Task<int> ProcessDueAlertsAsync(IGapPhaseARepository repository, DateTime utcNow, CancellationToken cancellationToken)
    {
        var alerts = await repository.GetOpenAlertsDueAsync(utcNow, cancellationToken);

        foreach (var alert in alerts)
        {
            alert.EscalationLevel += 1;
            alert.LastNotifiedDateUtc = utcNow;
            alert.UpdatedBy = "GapPhaseAWorker";
            alert.LastUpdated = utcNow;
            alert.Severity = alert.EscalationLevel >= 3 ? SystemAlertSeverity.Critical : SystemAlertSeverity.Warning;
            alert.SlaDueDateUtc = utcNow.AddMinutes(30);
        }

        return alerts.Count;
    }
}
