using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class ServiceRequestNumberGenerator : IServiceRequestNumberGenerator
{
    public string GenerateNumber()
    {
        return $"SR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
