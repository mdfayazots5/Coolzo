using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketHistory;

public sealed class GetSupportTicketHistoryQueryHandler : IRequestHandler<GetSupportTicketHistoryQuery, IReadOnlyCollection<SupportTicketStatusHistoryResponse>>
{
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetSupportTicketHistoryQueryHandler(SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<IReadOnlyCollection<SupportTicketStatusHistoryResponse>> Handle(GetSupportTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForReadAsync(request.SupportTicketId, cancellationToken);

        return SupportTicketResponseMapper.ToStatusHistoryResponses(supportTicket);
    }
}
