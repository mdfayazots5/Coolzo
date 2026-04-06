using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Features.Billing;

public sealed class BillingAccessService
{
    private static readonly string[] AdminRoles =
    [
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.OperationsManager,
        RoleNames.OperationsExecutive
    ];

    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public BillingAccessService(
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext,
        ITechnicianJobAccessService technicianJobAccessService)
    {
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
        _technicianJobAccessService = technicianJobAccessService;
    }

    public bool HasQuotationReadAccess()
    {
        return HasAnyRole(AdminRoles) || _currentUserContext.Permissions.Contains(PermissionNames.QuotationRead);
    }

    public bool HasInvoiceReadAccess()
    {
        return HasAnyRole(AdminRoles) || _currentUserContext.Permissions.Contains(PermissionNames.InvoiceRead);
    }

    public void EnsureInvoiceCreateAccess()
    {
        if (HasAnyRole(AdminRoles) || _currentUserContext.Permissions.Contains(PermissionNames.InvoiceCreate))
        {
            return;
        }

        throw new AppException(ErrorCodes.Forbidden, "The current user cannot generate invoices.", 403);
    }

    public bool HasPaymentCollectAccess()
    {
        return HasAnyRole(AdminRoles) || _currentUserContext.Permissions.Contains(PermissionNames.PaymentCollect);
    }

    public async Task EnsureCustomerOwnershipAsync(long customerId, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "An authenticated customer session is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (customer is null || customer.CustomerId != customerId)
        {
            throw new AppException(ErrorCodes.BillingAccessDenied, "The requested billing record does not belong to the current customer.", 403);
        }
    }

    public async Task EnsureTechnicianOwnershipAsync(JobCard? jobCard, CancellationToken cancellationToken)
    {
        if (jobCard?.ServiceRequest is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The requested job card could not be resolved.", 404);
        }

        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var activeAssignment = jobCard.ServiceRequest.Assignments
            .FirstOrDefault(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted);

        if (activeAssignment is null || activeAssignment.TechnicianId != technician.TechnicianId)
        {
            throw new AppException(ErrorCodes.BillingAccessDenied, "The current technician cannot access this billing flow.", 403);
        }
    }

    public async Task EnsureQuotationReadAccessAsync(QuotationHeader quotationHeader, CancellationToken cancellationToken)
    {
        if (HasQuotationReadAccess())
        {
            return;
        }

        if (_currentUserContext.Roles.Contains(RoleNames.Technician))
        {
            await EnsureTechnicianOwnershipAsync(quotationHeader.JobCard, cancellationToken);
            return;
        }

        await EnsureCustomerOwnershipAsync(quotationHeader.CustomerId, cancellationToken);
    }

    public async Task EnsureInvoiceReadAccessAsync(InvoiceHeader invoiceHeader, CancellationToken cancellationToken)
    {
        if (HasInvoiceReadAccess() || HasPaymentCollectAccess())
        {
            return;
        }

        await EnsureCustomerOwnershipAsync(invoiceHeader.CustomerId, cancellationToken);
    }

    private bool HasAnyRole(IReadOnlyCollection<string> roles)
    {
        return _currentUserContext.Roles.Any(roles.Contains);
    }
}
