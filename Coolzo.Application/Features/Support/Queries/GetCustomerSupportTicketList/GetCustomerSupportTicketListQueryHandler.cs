using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetCustomerSupportTicketList;

public sealed class GetCustomerSupportTicketListQueryHandler : IRequestHandler<GetCustomerSupportTicketListQuery, PagedResult<SupportTicketListItemResponse>>
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetCustomerSupportTicketListQueryHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<PagedResult<SupportTicketListItemResponse>> Handle(GetCustomerSupportTicketListQuery request, CancellationToken cancellationToken)
    {
        var customer = await _supportTicketAccessService.GetCurrentCustomerAsync(cancellationToken);
        var tickets = await _supportTicketRepository.ListByCustomerIdAsync(
            customer.CustomerId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var totalCount = await _supportTicketRepository.CountByCustomerIdAsync(customer.CustomerId, cancellationToken);

        return new PagedResult<SupportTicketListItemResponse>(
            tickets.Select(SupportTicketResponseMapper.ToListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
