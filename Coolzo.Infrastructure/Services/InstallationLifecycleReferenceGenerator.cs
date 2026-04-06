using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class InstallationLifecycleReferenceGenerator : IInstallationLifecycleReferenceGenerator
{
    public string GenerateInstallationNumber()
    {
        return $"INL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GenerateInstallationProposalNumber()
    {
        return $"INP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GenerateWarrantyRegistrationNumber()
    {
        return $"WAR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }
}
