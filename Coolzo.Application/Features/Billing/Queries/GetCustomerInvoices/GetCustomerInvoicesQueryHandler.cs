using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetCustomerInvoices;

public sealed class GetCustomerInvoicesQueryHandler : IRequestHandler<GetCustomerInvoicesQuery, PagedResult<InvoiceListItemResponse>>
{
    private readonly IBillingRepository _billingRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetCustomerInvoicesQueryHandler(
        IBillingRepository billingRepository,
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext)
    {
        _billingRepository = billingRepository;
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResult<InvoiceListItemResponse>> Handle(GetCustomerInvoicesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "An authenticated customer session is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.BillingAccessDenied, "The current customer profile could not be resolved.", 403);
        var invoices = await _billingRepository.SearchInvoicesAsync(null, customer.CustomerId, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _billingRepository.CountInvoicesAsync(null, customer.CustomerId, cancellationToken);

        return new PagedResult<InvoiceListItemResponse>(
            invoices.Select(BillingResponseMapper.ToInvoiceListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
