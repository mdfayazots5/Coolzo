using Coolzo.Shared.Models;

namespace Coolzo.Infrastructure.Services;

public sealed class SystemCurrentDateTime : ICurrentDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
