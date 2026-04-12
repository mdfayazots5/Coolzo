using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.CMS;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class CustomerContentController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerContentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("cms/public/blogs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BlogContentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BlogContentResponse>>>> GetBlogsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicBlogsQuery(), cancellationToken);
        return Success(response);
    }

    [HttpGet("cms/public/blogs/{id}")]
    [ProducesResponseType(typeof(ApiResponse<BlogContentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BlogContentResponse?>>> GetBlogByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicBlogByIdQuery(id), cancellationToken);
        return Success(response);
    }

    [HttpGet("cms/public/changelog")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ChangelogItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ChangelogItemResponse>>>> GetChangelogAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetPublicChangelogQuery(), cancellationToken);
        return Success(response);
    }

    [Authorize]
    [HttpPost("customer-app/feedback")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAppFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAppFeedbackResponse>>> SubmitFeedbackAsync(
        [FromBody] SubmitAppFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitMyAppFeedbackCommand(
                request.FeedbackType,
                request.Message,
                request.Rating,
                request.AppVersion,
                request.DeviceInfo),
            cancellationToken);

        return Success(response, "Customer app feedback submitted successfully.");
    }
}
