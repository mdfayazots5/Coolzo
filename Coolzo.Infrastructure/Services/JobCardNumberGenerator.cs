using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class JobCardNumberGenerator : IJobCardNumberGenerator
{
    public string GenerateNumber()
    {
        return $"JC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant()[..24];
    }
}
