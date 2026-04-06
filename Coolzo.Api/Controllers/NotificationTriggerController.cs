using Asp.Versioning;
using Coolzo.Application.Features.NotificationTriggerConfiguration.Commands.CreateNotificationTriggerConfiguration;
using Coolzo.Application.Features.NotificationTriggerConfiguration.Commands.UpdateNotificationTriggerConfiguration;
using Coolzo.Application.Features.NotificationTriggerConfiguration.Queries.GetNotificationTriggerList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/notification-triggers")]
public sealed class NotificationTriggerController : ApiControllerBase
{
    private readonly ISender _sender;

    public NotificationTriggerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.NotificationTriggerRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<NotificationTriggerConfigurationResponse>>>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isEnabled,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetNotificationTriggerListQuery(search, isEnabled), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.NotificationTriggerManage)]
    public async Task<ActionResult<ApiResponse<NotificationTriggerConfigurationResponse>>> CreateAsync(
        [FromBody] NotificationTriggerUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateNotificationTriggerConfigurationCommand(
                request.TriggerCode,
                request.TriggerName,
                request.Description,
                request.IsEnabled,
                request.EmailEnabled,
                request.SmsEnabled,
                request.WhatsAppEnabled,
                request.PushEnabled,
                request.ReminderLeadMinutes,
                request.DelayMinutes),
            cancellationToken);

        return Success(response, "Notification trigger created successfully.");
    }

    [HttpPut("{notificationTriggerConfigurationId:long}")]
    [Authorize(Policy = PermissionNames.NotificationTriggerManage)]
    public async Task<ActionResult<ApiResponse<NotificationTriggerConfigurationResponse>>> UpdateAsync(
        [FromRoute] long notificationTriggerConfigurationId,
        [FromBody] NotificationTriggerUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateNotificationTriggerConfigurationCommand(
                notificationTriggerConfigurationId,
                request.TriggerCode,
                request.TriggerName,
                request.Description,
                request.IsEnabled,
                request.EmailEnabled,
                request.SmsEnabled,
                request.WhatsAppEnabled,
                request.PushEnabled,
                request.ReminderLeadMinutes,
                request.DelayMinutes),
            cancellationToken);

        return Success(response, "Notification trigger updated successfully.");
    }
}
