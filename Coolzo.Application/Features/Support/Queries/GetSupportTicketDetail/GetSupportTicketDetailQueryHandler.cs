using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Enums;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketDetail;

public sealed class GetSupportTicketDetailQueryHandler : IRequestHandler<GetSupportTicketDetailQuery, SupportTicketDetailResponse>
{
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetSupportTicketDetailQueryHandler(SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<SupportTicketDetailResponse> Handle(GetSupportTicketDetailQuery request, CancellationToken cancellationToken)
    {
        var supportTicket = await _supportTicketAccessService.GetTicketForReadAsync(request.SupportTicketId, cancellationToken);
        var canManage = _supportTicketAccessService.CanManage();
        var canCustomerClose = _supportTicketAccessService.IsCustomer() && supportTicket.CurrentStatus == SupportTicketStatus.Resolved;

        return SupportTicketResponseMapper.ToDetail(
            supportTicket,
            canManage,
            canManage,
            canCustomerClose,
            canManage);
    }
}
