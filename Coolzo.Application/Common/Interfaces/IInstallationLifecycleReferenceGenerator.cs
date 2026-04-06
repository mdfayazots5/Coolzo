namespace Coolzo.Application.Common.Interfaces;

public interface IInstallationLifecycleReferenceGenerator
{
    string GenerateInstallationNumber();

    string GenerateInstallationProposalNumber();

    string GenerateWarrantyRegistrationNumber();
}
