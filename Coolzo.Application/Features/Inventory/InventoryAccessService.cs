using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Features.Inventory;

public sealed class InventoryAccessService
{
    private static readonly string[] ManagementRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager,
        RoleNames.OperationsExecutive
    ];

    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public InventoryAccessService(
        ICurrentUserContext currentUserContext,
        ITechnicianJobAccessService technicianJobAccessService)
    {
        _currentUserContext = currentUserContext;
        _technicianJobAccessService = technicianJobAccessService;
    }

    public bool HasStockReadAccess()
    {
        return HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.StockRead);
    }

    public bool HasJobConsumptionReadAccess()
    {
        return HasManagementRole() ||
            _currentUserContext.Permissions.Contains(PermissionNames.JobConsumptionRead) ||
            HasStockReadAccess();
    }

    public bool HasJobConsumptionCreateAccess()
    {
        return HasManagementRole() ||
            _currentUserContext.Permissions.Contains(PermissionNames.JobConsumptionCreate) ||
            _currentUserContext.Permissions.Contains(PermissionNames.StockManage);
    }

    public async Task EnsureTechnicianStockReadAccessAsync(long technicianId, CancellationToken cancellationToken)
    {
        if (HasStockReadAccess())
        {
            return;
        }

        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);

        if (technician.TechnicianId != technicianId)
        {
            throw new AppException(
                ErrorCodes.InventoryAccessDenied,
                "The current user cannot access another technician's stock.",
                403);
        }
    }

    public async Task EnsureJobConsumptionReadAccessAsync(JobCard jobCard, CancellationToken cancellationToken)
    {
        if (HasJobConsumptionReadAccess())
        {
            return;
        }

        await EnsureTechnicianOwnershipAsync(jobCard, cancellationToken);
    }

    public async Task EnsureJobConsumptionCreateAccessAsync(JobCard jobCard, CancellationToken cancellationToken)
    {
        if (HasJobConsumptionCreateAccess())
        {
            return;
        }

        await EnsureTechnicianOwnershipAsync(jobCard, cancellationToken);
    }

    private bool HasManagementRole()
    {
        return _currentUserContext.Roles.Any(ManagementRoles.Contains);
    }

    private async Task EnsureTechnicianOwnershipAsync(JobCard jobCard, CancellationToken cancellationToken)
    {
        if (jobCard.ServiceRequest is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The requested job card could not be resolved.", 404);
        }

        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var activeAssignment = jobCard.ServiceRequest.Assignments
            .FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment is null || activeAssignment.TechnicianId != technician.TechnicianId)
        {
            throw new AppException(
                ErrorCodes.InventoryAccessDenied,
                "The current technician is not assigned to this job's inventory flow.",
                403);
        }
    }
}
