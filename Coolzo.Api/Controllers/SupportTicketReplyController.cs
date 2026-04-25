using Coolzo.Application.Features.Support.Commands.AddSupportTicketReply;
using Coolzo.Application.Features.Support.Queries.GetSupportTicketReplies;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/support-tickets/{supportTicketId:long}/replies")]
public sealed class SupportTicketReplyController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupportTicketReplyController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SupportTicketReplyResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SupportTicketReplyResponse>>>> GetAsync(
        [FromRoute] long supportTicketId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSupportTicketRepliesQuery(supportTicketId), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketReplyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportTicketReplyResponse>>> CreateAsync(
        [FromRoute] long supportTicketId,
        [FromBody] AddSupportTicketReplyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AddSupportTicketReplyCommand(supportTicketId, request.ReplyText, request.IsInternalOnly),
            cancellationToken);

        return Success(response, "Support ticket reply added successfully.");
    }
}
