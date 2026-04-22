using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.SearchInvoices;

public sealed class SearchInvoicesQueryHandler : IRequestHandler<SearchInvoicesQuery, PagedResult<InvoiceListItemResponse>>
{
    private readonly IBillingRepository _billingRepository;

    public SearchInvoicesQueryHandler(IBillingRepository billingRepository)
    {
        _billingRepository = billingRepository;
    }

    public async Task<PagedResult<InvoiceListItemResponse>> Handle(SearchInvoicesQuery request, CancellationToken cancellationToken)
    {
        var status = ParseStatus(request.Status);
        var invoices = await _billingRepository.SearchInvoicesAsync(status, request.CustomerId, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _billingRepository.CountInvoicesAsync(status, request.CustomerId, cancellationToken);

        return new PagedResult<InvoiceListItemResponse>(
            invoices.Select(BillingResponseMapper.ToInvoiceListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static InvoicePaymentStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<InvoicePaymentStatus>(status, true, out var parsedStatus))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Invoice status filter is invalid.", 400);
        }

        return parsedStatus;
    }
}
