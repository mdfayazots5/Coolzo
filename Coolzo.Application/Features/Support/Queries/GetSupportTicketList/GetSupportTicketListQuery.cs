using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketList;

public sealed record GetSupportTicketListQuery(
    string? TicketNumber,
    string? CustomerMobile,
    long? CategoryId,
    long? PriorityId,
    string? Status,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? LinkedEntityType,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<SupportTicketListItemResponse>>;
