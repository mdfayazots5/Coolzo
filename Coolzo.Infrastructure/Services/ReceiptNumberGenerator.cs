using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class ReceiptNumberGenerator : IReceiptNumberGenerator
{
    public string GenerateNumber()
    {
        return $"RCT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
