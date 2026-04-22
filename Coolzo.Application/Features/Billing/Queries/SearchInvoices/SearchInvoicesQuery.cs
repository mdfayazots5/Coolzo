using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.SearchInvoices;

public sealed record SearchInvoicesQuery(
    string? Status,
    long? CustomerId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<InvoiceListItemResponse>>;
