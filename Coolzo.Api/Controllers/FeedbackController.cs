using Coolzo.Application.Features.Support.Commands.FlagFeedback;
using Coolzo.Application.Features.Support.Commands.PublishFeedback;
using Coolzo.Application.Features.Support.Commands.RespondFeedback;
using Coolzo.Application.Features.Support.Queries.GetFeedbackDetail;
using Coolzo.Application.Features.Support.Queries.GetFeedbackList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/feedback")]
public sealed class FeedbackController : ApiControllerBase
{
    private readonly ISender _sender;

    public FeedbackController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.SupportRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SupportFeedbackResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SupportFeedbackResponse>>>> GetAsync(
        [FromQuery] long? serviceId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetFeedbackListQuery(serviceId), cancellationToken);
        return Success(response);
    }

    [HttpGet("{customerReviewId:long}")]
    [Authorize(Policy = PermissionNames.SupportRead)]
    [ProducesResponseType(typeof(ApiResponse<SupportFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportFeedbackResponse>>> GetByIdAsync(
        [FromRoute] long customerReviewId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetFeedbackDetailQuery(customerReviewId), cancellationToken);
        return Success(response);
    }

    [HttpPatch("{customerReviewId:long}/respond")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportFeedbackResponse>>> RespondAsync(
        [FromRoute] long customerReviewId,
        [FromBody] RespondFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RespondFeedbackCommand(customerReviewId, request.Response ?? string.Empty), cancellationToken);
        return Success(response, "Feedback response saved successfully.");
    }

    [HttpPatch("{customerReviewId:long}/publish")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportFeedbackResponse>>> PublishAsync(
        [FromRoute] long customerReviewId,
        [FromBody] PublishFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new PublishFeedbackCommand(customerReviewId, request.Publish), cancellationToken);
        return Success(response, request.Publish ? "Feedback published successfully." : "Feedback unpublished successfully.");
    }

    [HttpPatch("{customerReviewId:long}/flag")]
    [Authorize(Policy = PermissionNames.SupportManage)]
    [ProducesResponseType(typeof(ApiResponse<SupportFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupportFeedbackResponse>>> FlagAsync(
        [FromRoute] long customerReviewId,
        [FromBody] FlagFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new FlagFeedbackCommand(customerReviewId, request.Reason), cancellationToken);
        return Success(response, "Feedback flagged successfully.");
    }
}
