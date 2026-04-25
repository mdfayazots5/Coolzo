using Coolzo.Application.Features.Support.Commands.EscalateSupportTicket;
using Coolzo.Application.Features.Support.Queries.GetSupportTicketEscalations;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/support-tickets/{supportTicketId:long}")]
public sealed class SupportTicketEscalationController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupportTicketEscalationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("escalations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SupportTicketEscalationResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SupportTicketEscalationResponse>>>> GetAsync(
        [FromRoute] long supportTicketId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketEscalationsQuery(supportTicketId), cancellationToken);

        return Success(response);
    }

    [HttpPost("escalate")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailResponse>>> EscalateAsync(
        [FromRoute] long supportTicketId,
        [FromBody] EscalateSupportTicketRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new EscalateSupportTicketCommand(supportTicketId, request.EscalationTarget, request.EscalationRemarks),
            cancellationToken);

        return Success(response, "Support ticket escalated successfully.");
    }
}
