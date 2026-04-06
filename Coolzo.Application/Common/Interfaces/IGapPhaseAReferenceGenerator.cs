namespace Coolzo.Application.Common.Interfaces;

public interface IGapPhaseAReferenceGenerator
{
    string GenerateLeadNumber();

    string GenerateInstallationOrderNumber();

    string GenerateCampaignCode();

    string GeneratePartsReturnNumber();

    string GenerateCommissioningCertificateNumber();
}
