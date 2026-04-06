using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetCustomerSupportTicketList;

public sealed record GetCustomerSupportTicketListQuery(
    int PageNumber,
    int PageSize) : IRequest<PagedResult<SupportTicketListItemResponse>>;
