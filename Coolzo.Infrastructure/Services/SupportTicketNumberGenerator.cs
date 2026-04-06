using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class SupportTicketNumberGenerator : ISupportTicketNumberGenerator
{
    public string GenerateNumber()
    {
        return $"ST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
