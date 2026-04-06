using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.Amc;

public sealed class ServiceLifecycleAccessService
{
    private static readonly string[] AdminRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager,
        RoleNames.OperationsExecutive,
        RoleNames.CustomerSupportExecutive
    ];

    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ServiceLifecycleAccessService(
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public void EnsureAmcPlanCreateAccess()
    {
        if (HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.AmcCreate))
        {
            return;
        }

        throw new AppException(ErrorCodes.Forbidden, "The current user cannot create AMC plans.", 403);
    }

    public void EnsureAmcAssignmentAccess()
    {
        if (HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.AmcAssign))
        {
            return;
        }

        throw new AppException(ErrorCodes.Forbidden, "The current user cannot assign AMC subscriptions.", 403);
    }

    public bool HasWarrantyReadAccess()
    {
        return HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.WarrantyRead);
    }

    public bool HasRevisitReadAccess()
    {
        return HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.RevisitRead);
    }

    public bool HasServiceHistoryReadAccess()
    {
        return HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.ServiceHistoryRead);
    }

    public async Task EnsureCustomerOwnershipAsync(long customerId, CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);

        if (customer is null || customer.CustomerId != customerId)
        {
            throw new AppException(
                ErrorCodes.LifecycleAccessDenied,
                "The requested lifecycle record does not belong to the current customer.",
                403);
        }
    }

    public async Task<long> GetCurrentCustomerIdAsync(CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);

        return customer?.CustomerId
            ?? throw new AppException(ErrorCodes.NotFound, "The current user is not linked to a customer profile.", 404);
    }

    public async Task EnsureCustomerReadAccessAsync(long customerId, CancellationToken cancellationToken)
    {
        if (HasManagementRole())
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(customerId, cancellationToken);
    }

    public async Task EnsureBookingReadAccessAsync(DomainBooking booking, CancellationToken cancellationToken)
    {
        if (HasManagementRole() || HasRevisitReadAccess())
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(booking.CustomerId, cancellationToken);
    }

    public async Task EnsureInvoiceReadAccessAsync(InvoiceHeader invoiceHeader, CancellationToken cancellationToken)
    {
        if (HasManagementRole() || HasWarrantyReadAccess())
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(invoiceHeader.CustomerId, cancellationToken);
    }

    public async Task EnsureWarrantyClaimCreateAccessAsync(InvoiceHeader invoiceHeader, CancellationToken cancellationToken)
    {
        if (HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.WarrantyClaim))
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(invoiceHeader.CustomerId, cancellationToken);
    }

    public async Task EnsureRevisitCreateAccessAsync(DomainBooking booking, CancellationToken cancellationToken)
    {
        if (HasManagementRole() || _currentUserContext.Permissions.Contains(PermissionNames.RevisitCreate))
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(booking.CustomerId, cancellationToken);
    }

    private bool HasManagementRole()
    {
        return _currentUserContext.Roles.Any(AdminRoles.Contains);
    }

    private async Task<Coolzo.Domain.Entities.Customer?> GetCurrentCustomerAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "An authenticated session is required.", 401);
        }

        return await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken);
    }
}
