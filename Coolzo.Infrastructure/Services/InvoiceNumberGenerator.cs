using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    public string GenerateNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
