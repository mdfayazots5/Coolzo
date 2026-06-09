using Coolzo.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Coolzo.Api.HealthChecks;

public sealed class ObjectStorageHealthCheck : IHealthCheck
{
    private readonly IObjectStorageService _objectStorageService;

    public ObjectStorageHealthCheck(IObjectStorageService objectStorageService)
    {
        _objectStorageService = objectStorageService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isWritable = await _objectStorageService.CheckWritableAsync(cancellationToken);

            return isWritable
                ? HealthCheckResult.Healthy("Object storage is reachable and writable.")
                : HealthCheckResult.Unhealthy("Object storage is not writable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Object storage health check failed.", ex);
        }
    }
}
