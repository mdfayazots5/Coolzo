using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Features.Support;

public sealed class SupportTicketAccessService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISupportTicketRepository _supportTicketRepository;

    public SupportTicketAccessService(
        ICurrentUserContext currentUserContext,
        ISupportTicketRepository supportTicketRepository)
    {
        _currentUserContext = currentUserContext;
        _supportTicketRepository = supportTicketRepository;
    }

    public bool CanReadAll()
    {
        return CanManage() || HasPermission(PermissionNames.SupportRead);
    }

    public bool CanManage()
    {
        return HasPermission(PermissionNames.SupportManage);
    }

    public bool IsCustomer()
    {
        return _currentUserContext.Roles.Any(role => string.Equals(role, RoleNames.Customer, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Customer> GetCurrentCustomerAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "A signed-in customer context is required.", 403);
        }

        return await _supportTicketRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.SupportAccessDenied, "The current customer profile could not be resolved.", 403);
    }

    public async Task<SupportTicket> GetTicketForReadAsync(long supportTicketId, CancellationToken cancellationToken)
    {
        return await GetTicketAsync(supportTicketId, false, cancellationToken);
    }

    public async Task<SupportTicket> GetTicketForUpdateAsync(long supportTicketId, CancellationToken cancellationToken)
    {
        return await GetTicketAsync(supportTicketId, true, cancellationToken);
    }

    public void EnsureCanManage()
    {
        if (!CanManage())
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "The current user is not allowed to manage support tickets.", 403);
        }
    }

    private async Task<SupportTicket> GetTicketAsync(long supportTicketId, bool tracked, CancellationToken cancellationToken)
    {
        var supportTicket = tracked
            ? await _supportTicketRepository.GetByIdForUpdateAsync(supportTicketId, cancellationToken)
            : await _supportTicketRepository.GetByIdAsync(supportTicketId, cancellationToken);

        if (supportTicket is null)
        {
            throw new AppException(ErrorCodes.NotFound, "The requested support ticket could not be found.", 404);
        }

        if (CanReadAll())
        {
            return supportTicket;
        }

        if (!IsCustomer())
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "The current user is not allowed to access this support ticket.", 403);
        }

        var customer = await GetCurrentCustomerAsync(cancellationToken);

        if (supportTicket.CustomerId != customer.CustomerId)
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "The requested support ticket does not belong to the current customer.", 403);
        }

        return supportTicket;
    }

    private bool HasPermission(string permissionName)
    {
        return _currentUserContext.Permissions.Any(permission =>
            string.Equals(permission, permissionName, StringComparison.OrdinalIgnoreCase));
    }
}
