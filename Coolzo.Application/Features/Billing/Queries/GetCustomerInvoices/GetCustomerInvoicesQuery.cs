using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Billing;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetCustomerInvoices;

public sealed record GetCustomerInvoicesQuery(
    int PageNumber,
    int PageSize) : IRequest<PagedResult<InvoiceListItemResponse>>;
