using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IInstallationLifecycleRepository
{
    Task AddInstallationAsync(InstallationLead installation, CancellationToken cancellationToken);

    Task<bool> InstallationNumberExistsAsync(string installationNumber, CancellationToken cancellationToken);

    Task<bool> ProposalNumberExistsAsync(string proposalNumber, CancellationToken cancellationToken);

    Task<bool> WarrantyRegistrationNumberExistsAsync(string warrantyRegistrationNumber, CancellationToken cancellationToken);

    Task<InstallationLead?> GetInstallationByIdAsync(long installationId, CancellationToken cancellationToken);

    Task<InstallationLead?> GetInstallationByIdForUpdateAsync(long installationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InstallationLead>> SearchInstallationsAsync(
        string? searchTerm,
        InstallationLifecycleStatus? installationStatus,
        InstallationApprovalStatus? approvalStatus,
        long? customerId,
        long? technicianId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountInstallationsAsync(
        string? searchTerm,
        InstallationLifecycleStatus? installationStatus,
        InstallationApprovalStatus? approvalStatus,
        long? customerId,
        long? technicianId,
        CancellationToken cancellationToken);
}
