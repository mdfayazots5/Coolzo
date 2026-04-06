using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class InstallationLifecycleRepository : IInstallationLifecycleRepository
{
    private readonly CoolzoDbContext _dbContext;

    public InstallationLifecycleRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddInstallationAsync(InstallationLead installation, CancellationToken cancellationToken)
    {
        return _dbContext.InstallationLeads.AddAsync(installation, cancellationToken).AsTask();
    }

    public Task<bool> InstallationNumberExistsAsync(string installationNumber, CancellationToken cancellationToken)
    {
        return _dbContext.InstallationLeads.AnyAsync(
            entity => !entity.IsDeleted && entity.InstallationNumber == installationNumber,
            cancellationToken);
    }

    public Task<bool> ProposalNumberExistsAsync(string proposalNumber, CancellationToken cancellationToken)
    {
        return _dbContext.InstallationProposals.AnyAsync(
            entity => !entity.IsDeleted && entity.ProposalNumber == proposalNumber,
            cancellationToken);
    }

    public Task<bool> WarrantyRegistrationNumberExistsAsync(string warrantyRegistrationNumber, CancellationToken cancellationToken)
    {
        return _dbContext.CommissioningCertificates.AnyAsync(
            entity => !entity.IsDeleted && entity.WarrantyRegistrationNumber == warrantyRegistrationNumber,
            cancellationToken);
    }

    public Task<InstallationLead?> GetInstallationByIdAsync(long installationId, CancellationToken cancellationToken)
    {
        return BuildInstallationQuery(asNoTracking: true)
            .FirstOrDefaultAsync(entity => entity.InstallationId == installationId, cancellationToken);
    }

    public Task<InstallationLead?> GetInstallationByIdForUpdateAsync(long installationId, CancellationToken cancellationToken)
    {
        return BuildInstallationQuery(asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.InstallationId == installationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InstallationLead>> SearchInstallationsAsync(
        string? searchTerm,
        InstallationLifecycleStatus? installationStatus,
        InstallationApprovalStatus? approvalStatus,
        long? customerId,
        long? technicianId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await ApplyFilters(
                BuildInstallationQuery(asNoTracking: true),
                searchTerm,
                installationStatus,
                approvalStatus,
                customerId,
                technicianId)
            .OrderByDescending(entity => entity.DateCreated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountInstallationsAsync(
        string? searchTerm,
        InstallationLifecycleStatus? installationStatus,
        InstallationApprovalStatus? approvalStatus,
        long? customerId,
        long? technicianId,
        CancellationToken cancellationToken)
    {
        return ApplyFilters(
                _dbContext.InstallationLeads.Where(entity => !entity.IsDeleted),
                searchTerm,
                installationStatus,
                approvalStatus,
                customerId,
                technicianId)
            .CountAsync(cancellationToken);
    }

    private IQueryable<InstallationLead> BuildInstallationQuery(bool asNoTracking)
    {
        IQueryable<InstallationLead> query = _dbContext.InstallationLeads
            .Include(entity => entity.Lead)
            .Include(entity => entity.Customer)
            .Include(entity => entity.CustomerAddress)
            .Include(entity => entity.AssignedTechnician)
            .Include(entity => entity.Surveys.Where(survey => !survey.IsDeleted))
                .ThenInclude(survey => survey.Items.Where(item => !item.IsDeleted))
            .Include(entity => entity.Surveys.Where(survey => !survey.IsDeleted))
                .ThenInclude(survey => survey.Technician)
            .Include(entity => entity.Proposals.Where(proposal => !proposal.IsDeleted))
                .ThenInclude(proposal => proposal.Lines.Where(line => !line.IsDeleted))
            .Include(entity => entity.Checklists.Where(checklist => !checklist.IsDeleted))
                .ThenInclude(checklist => checklist.Responses.Where(response => !response.IsDeleted))
            .Include(entity => entity.StatusHistories.Where(history => !history.IsDeleted))
            .Include(entity => entity.Orders.Where(order => !order.IsDeleted))
                .ThenInclude(order => order.Technician)
            .Include(entity => entity.CommissioningCertificates.Where(certificate => !certificate.IsDeleted))
            .Where(entity => !entity.IsDeleted);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private static IQueryable<InstallationLead> ApplyFilters(
        IQueryable<InstallationLead> query,
        string? searchTerm,
        InstallationLifecycleStatus? installationStatus,
        InstallationApprovalStatus? approvalStatus,
        long? customerId,
        long? technicianId)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();
            query = query.Where(entity =>
                entity.InstallationNumber.Contains(normalizedSearch)
                || entity.Customer!.CustomerName.Contains(normalizedSearch)
                || entity.Customer.MobileNumber.Contains(normalizedSearch)
                || entity.InstallationType.Contains(normalizedSearch));
        }

        if (installationStatus.HasValue)
        {
            query = query.Where(entity => entity.InstallationStatus == installationStatus.Value);
        }

        if (approvalStatus.HasValue)
        {
            query = query.Where(entity => entity.ApprovalStatus == approvalStatus.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(entity => entity.CustomerId == customerId.Value);
        }

        if (technicianId.HasValue)
        {
            query = query.Where(entity => entity.AssignedTechnicianId == technicianId.Value);
        }

        return query;
    }
}
