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
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerNotificationResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerNotificationResponse>>>> GetUnreadAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetMyNotificationsQuery(pageNumber, Math.Clamp(pageSize, 1, 500)), cancellationToken);
        var unreadItems = response.Items.Where(item => !item.IsRead).ToArray();
        var unread = new PagedResult<CustomerNotificationResponse>(
            unreadItems,
            unreadItems.Length,
            response.PageNumber,
            response.PageSize);

        return Success(unread);
    }

    [HttpPatch("mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyNotificationsQuery(1, 500), cancellationToken);
        var unread = response.Items.Where(item => !item.IsRead).ToArray();

        foreach (var notification in unread)
        {
            await _sender.Send(new MarkMyNotificationReadCommand(notification.CustomerNotificationId), cancellationToken);
        }

        return Success<object>(new { count = unread.Length }, "All unread notifications marked as read.");
    }
}
