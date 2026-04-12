using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/customer-notifications")]
public sealed class CustomerNotificationController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerNotificationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerNotificationResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerNotificationResponse>>>> GetMineAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetMyNotificationsQuery(pageNumber, pageSize), cancellationToken);
        return Success(response);
    }

    [HttpPost("{notificationId:long}/mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> MarkReadAsync(
        [FromRoute] long notificationId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkMyNotificationReadCommand(notificationId), cancellationToken);
        return Success<object>(new { notificationId }, "Notification marked as read.");
    }
}
