using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketReplies;

public sealed class GetSupportTicketRepliesQueryHandler : IRequestHandler<GetSupportTicketRepliesQuery, IReadOnlyCollection<SupportTicketReplyResponse>>
{
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetSupportTicketRepliesQueryHandler(SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<IReadOnlyCollection<SupportTicketReplyResponse>> Handle(GetSupportTicketRepliesQuery request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForReadAsync(request.SupportTicketId, cancellationToken);

        return SupportTicketResponseMapper.ToReplyResponses(supportTicket, _supportTicketAccessService.CanManage());
    }
}
