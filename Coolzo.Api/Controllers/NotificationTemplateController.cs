using Coolzo.Application.Features.NotificationTemplate.Commands.CreateNotificationTemplate;
using Coolzo.Application.Features.NotificationTemplate.Commands.UpdateNotificationTemplate;
using Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateDetail;
using Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/notification-templates")]
public sealed class NotificationTemplateController : ApiControllerBase
{
    private readonly ISender _sender;

    public NotificationTemplateController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.NotificationTemplateRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<NotificationTemplateResponse>>>> GetAsync(
        [FromQuery] string? search,
        [FromQuery] string? channel,
        [FromQuery] string? triggerCode,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetNotificationTemplateListQuery(search, channel, triggerCode, isActive), cancellationToken);

        return Success(response);
    }

    [HttpGet("{notificationTemplateId:long}")]
    [Authorize(Policy = PermissionNames.NotificationTemplateRead)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateResponse>>> GetDetailAsync(
        [FromRoute] long notificationTemplateId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetNotificationTemplateDetailQuery(notificationTemplateId), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.NotificationTemplateManage)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateResponse>>> CreateAsync(
        [FromBody] NotificationTemplateUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateNotificationTemplateCommand(
                request.TemplateCode,
                request.TemplateName,
                request.TriggerCode,
                request.Channel,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.AllowedMergeTags,
                request.IsActive),
            cancellationToken);

        return Success(response, "Notification template created successfully.");
    }

    [HttpPut("{notificationTemplateId:long}")]
    [Authorize(Policy = PermissionNames.NotificationTemplateManage)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateResponse>>> UpdateAsync(
        [FromRoute] long notificationTemplateId,
        [FromBody] NotificationTemplateUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateNotificationTemplateCommand(
                notificationTemplateId,
                request.TemplateCode,
                request.TemplateName,
                request.TriggerCode,
                request.Channel,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.AllowedMergeTags,
                request.IsActive),
            cancellationToken);

        return Success(response, "Notification template updated successfully.");
    }
}
