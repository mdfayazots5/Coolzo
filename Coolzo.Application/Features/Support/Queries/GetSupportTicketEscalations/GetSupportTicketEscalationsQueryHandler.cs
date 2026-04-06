using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketEscalations;

public sealed class GetSupportTicketEscalationsQueryHandler : IRequestHandler<GetSupportTicketEscalationsQuery, IReadOnlyCollection<SupportTicketEscalationResponse>>
{
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetSupportTicketEscalationsQueryHandler(SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<IReadOnlyCollection<SupportTicketEscalationResponse>> Handle(GetSupportTicketEscalationsQuery request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForReadAsync(request.SupportTicketId, cancellationToken);

        return _supportTicketAccessService.CanManage()
            ? SupportTicketResponseMapper.ToEscalationResponses(supportTicket)
            : Array.Empty<SupportTicketEscalationResponse>();
    }
}
