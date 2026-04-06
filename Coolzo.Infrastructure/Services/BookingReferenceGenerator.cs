using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class BookingReferenceGenerator : IBookingReferenceGenerator
{
    public string GenerateReference()
    {
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}
