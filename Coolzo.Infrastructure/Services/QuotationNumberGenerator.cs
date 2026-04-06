using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class QuotationNumberGenerator : IQuotationNumberGenerator
{
    public string GenerateNumber()
    {
        return $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
