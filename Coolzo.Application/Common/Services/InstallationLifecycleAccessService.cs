using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Common.Services;

public sealed class InstallationLifecycleAccessService
{
    private static readonly string[] ManagementRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager,
        RoleNames.OperationsExecutive,
        RoleNames.CustomerSupportExecutive
    ];

    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public InstallationLifecycleAccessService(
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext,
        ITechnicianJobAccessService technicianJobAccessService)
    {
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
        _technicianJobAccessService = technicianJobAccessService;
    }

    public bool HasManagementAccess()
    {
        return _currentUserContext.Roles.Any(ManagementRoles.Contains)
            || _currentUserContext.Permissions.Contains(PermissionNames.ServiceRequestRead)
            || _currentUserContext.Permissions.Contains(PermissionNames.ServiceRequestUpdate);
    }

    public async Task<long?> GetScopedCustomerIdAsync(CancellationToken cancellationToken)
    {
        if (HasManagementAccess() || !_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            return null;
        }

        return (await ResolveCurrentCustomerAsync(cancellationToken)).CustomerId;
    }

    public async Task<long?> GetScopedTechnicianIdAsync(CancellationToken cancellationToken)
    {
        if (HasManagementAccess() || !_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            return null;
        }

        return (await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken)).TechnicianId;
    }

    public async Task EnsureReadAccessAsync(InstallationLead installation, CancellationToken cancellationToken)
    {
        if (HasManagementAccess())
        {
            return;
        }

        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            var customer = await ResolveCurrentCustomerAsync(cancellationToken);

            if (customer.CustomerId == installation.CustomerId)
            {
                return;
            }

            throw new AppException(ErrorCodes.LifecycleAccessDenied, "The requested installation does not belong to the current customer.", 403);
        }

        if (_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);

            if (installation.AssignedTechnicianId == technician.TechnicianId)
            {
                return;
            }

            throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The current technician is not assigned to this installation.", 403);
        }

        throw new AppException(ErrorCodes.Forbidden, "The current user cannot access this installation.", 403);
    }

    public async Task EnsureProposalDecisionAccessAsync(InstallationLead installation, CancellationToken cancellationToken)
    {
        if (HasManagementAccess())
        {
            return;
        }

        if (_currentUserContext.Roles.Contains(RoleNames.Customer))
        {
            var customer = await ResolveCurrentCustomerAsync(cancellationToken);

            if (customer.CustomerId == installation.CustomerId)
            {
                return;
            }
        }

        throw new AppException(ErrorCodes.LifecycleAccessDenied, "The current user cannot approve or reject this installation proposal.", 403);
    }

    public async Task EnsureExecutionAccessAsync(InstallationLead installation, CancellationToken cancellationToken)
    {
        if (HasManagementAccess())
        {
            return;
        }

        if (_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);

            if (installation.AssignedTechnicianId == technician.TechnicianId)
            {
                return;
            }
        }

        throw new AppException(ErrorCodes.TechnicianJobAccessDenied, "The current user cannot update this installation execution.", 403);
    }

    private async Task<Customer> ResolveCurrentCustomerAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "An authenticated session is required.", 401);
        }

        return await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The current user is not linked to a customer profile.", 404);
    }
}
