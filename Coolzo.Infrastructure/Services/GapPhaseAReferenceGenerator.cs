using Coolzo.Application.Common.Interfaces;

namespace Coolzo.Infrastructure.Services;

public sealed class GapPhaseAReferenceGenerator : IGapPhaseAReferenceGenerator
{
    public string GenerateLeadNumber()
    {
        return $"LD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GenerateInstallationOrderNumber()
    {
        return $"INS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GenerateCampaignCode()
    {
        return $"CMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GeneratePartsReturnNumber()
    {
        return $"RMA-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public string GenerateCommissioningCertificateNumber()
    {
        return $"COM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }
}
