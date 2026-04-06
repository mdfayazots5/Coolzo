using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.SearchQuotations;

public sealed class SearchQuotationsQueryHandler : IRequestHandler<SearchQuotationsQuery, PagedResult<QuotationListItemResponse>>
{
    private readonly IBillingRepository _billingRepository;

    public SearchQuotationsQueryHandler(IBillingRepository billingRepository)
    {
        _billingRepository = billingRepository;
    }

    public async Task<PagedResult<QuotationListItemResponse>> Handle(SearchQuotationsQuery request, CancellationToken cancellationToken)
    {
        var status = ParseStatus(request.Status);
        var quotations = await _billingRepository.SearchQuotationsAsync(status, null, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _billingRepository.CountQuotationsAsync(status, null, cancellationToken);

        return new PagedResult<QuotationListItemResponse>(
            quotations.Select(BillingResponseMapper.ToQuotationListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static QuotationStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<QuotationStatus>(status, true, out var parsedStatus))
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Quotation status filter is invalid.", 400);
        }

        return parsedStatus;
    }
}
