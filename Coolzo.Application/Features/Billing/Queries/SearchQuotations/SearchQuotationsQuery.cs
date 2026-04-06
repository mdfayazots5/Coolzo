using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.SearchQuotations;

public sealed record SearchQuotationsQuery(
    string? Status,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<QuotationListItemResponse>>;
