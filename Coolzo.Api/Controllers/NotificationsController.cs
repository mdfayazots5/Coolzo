using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Application.Features.NotificationTemplate.Commands.UpdateNotificationTemplate;
using Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateDetail;
using Coolzo.Application.Features.NotificationTemplate.Queries.GetNotificationTemplateList;
using Coolzo.Application.Features.NotificationTriggerConfiguration.Commands.UpdateNotificationTriggerConfiguration;
using Coolzo.Application.Features.NotificationTriggerConfiguration.Queries.GetNotificationTriggerList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Contracts.Responses.Customer;
using Coolzo.Shared.Constants;
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

    [HttpGet("templates")]
    [Authorize(Policy = PermissionNames.NotificationTemplateRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<GovernanceNotificationTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GovernanceNotificationTemplateResponse>>>> GetTemplatesAsync(
        CancellationToken cancellationToken)
    {
        var templates = await _sender.Send(new GetNotificationTemplateListQuery(null, null, null, null), cancellationToken);
        var triggers = await _sender.Send(new GetNotificationTriggerListQuery(null, null), cancellationToken);

        return Success(MapGovernanceTemplates(templates, triggers));
    }

    [HttpPut("templates/{notificationTemplateId:long}")]
    [Authorize(Policy = PermissionNames.NotificationTemplateManage)]
    [ProducesResponseType(typeof(ApiResponse<GovernanceNotificationTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GovernanceNotificationTemplateResponse>>> UpdateTemplateAsync(
        [FromRoute] long notificationTemplateId,
        [FromBody] GovernanceNotificationTemplateUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var existingTemplate = await _sender.Send(new GetNotificationTemplateDetailQuery(notificationTemplateId), cancellationToken);

        var updatedTemplate = await _sender.Send(
            new UpdateNotificationTemplateCommand(
                notificationTemplateId,
                existingTemplate.TemplateCode,
                existingTemplate.TemplateName,
                existingTemplate.TriggerCode,
                existingTemplate.Channel,
                request.Subject ?? existingTemplate.SubjectTemplate,
                request.Body ?? existingTemplate.BodyTemplate,
                request.MergeTags?.ToArray() ?? existingTemplate.AllowedMergeTags,
                request.IsEnabled ?? existingTemplate.IsActive),
            cancellationToken);

        var triggers = await _sender.Send(new GetNotificationTriggerListQuery(existingTemplate.TriggerCode, null), cancellationToken);
        var matchingTrigger = triggers.FirstOrDefault(
            item => string.Equals(item.TriggerCode, existingTemplate.TriggerCode, StringComparison.OrdinalIgnoreCase));

        NotificationTriggerConfigurationResponse? updatedTrigger = matchingTrigger;

        if (matchingTrigger is not null && request.ChannelToggles is not null)
        {
            updatedTrigger = await _sender.Send(
                new UpdateNotificationTriggerConfigurationCommand(
                    matchingTrigger.NotificationTriggerConfigurationId,
                    matchingTrigger.TriggerCode,
                    matchingTrigger.TriggerName,
                    matchingTrigger.Description,
                    request.IsEnabled ?? matchingTrigger.IsEnabled,
                    request.ChannelToggles.TryGetValue("email", out var emailEnabled) ? emailEnabled : matchingTrigger.EmailEnabled,
                    request.ChannelToggles.TryGetValue("sms", out var smsEnabled) ? smsEnabled : matchingTrigger.SmsEnabled,
                    request.ChannelToggles.TryGetValue("whatsapp", out var whatsAppEnabled) ? whatsAppEnabled : matchingTrigger.WhatsAppEnabled,
                    request.ChannelToggles.TryGetValue("push", out var pushEnabled) ? pushEnabled : matchingTrigger.PushEnabled,
                    matchingTrigger.ReminderLeadMinutes,
                    matchingTrigger.DelayMinutes),
                cancellationToken);
        }

        var response = MapGovernanceTemplate(updatedTemplate, updatedTrigger);

        return Success(response, "Notification template updated successfully.");
    }

    [HttpGet("log")]
    [Authorize(Policy = PermissionNames.NotificationTemplateRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<GovernanceNotificationSendLogResponse>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IReadOnlyCollection<GovernanceNotificationSendLogResponse>>> GetNotificationLogAsync()
    {
        return Success<IReadOnlyCollection<GovernanceNotificationSendLogResponse>>(Array.Empty<GovernanceNotificationSendLogResponse>());
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

    private static IReadOnlyCollection<GovernanceNotificationTemplateResponse> MapGovernanceTemplates(
        IReadOnlyCollection<NotificationTemplateResponse> templates,
        IReadOnlyCollection<NotificationTriggerConfigurationResponse> triggers)
    {
        return templates
            .Select(template => MapGovernanceTemplate(
                template,
                triggers.FirstOrDefault(trigger =>
                    string.Equals(trigger.TriggerCode, template.TriggerCode, StringComparison.OrdinalIgnoreCase))))
            .ToArray();
    }

    private static GovernanceNotificationTemplateResponse MapGovernanceTemplate(
        NotificationTemplateResponse template,
        NotificationTriggerConfigurationResponse? trigger)
    {
        var lastUpdated = template.LastUpdated ?? template.DateCreated;
        var isEnabled = trigger?.IsEnabled ?? template.IsActive;

        return new GovernanceNotificationTemplateResponse(
            template.NotificationTemplateId.ToString(),
            string.IsNullOrWhiteSpace(trigger?.TriggerName) ? template.TriggerCode : trigger.TriggerName,
            NormalizeChannel(template.Channel),
            "customer",
            string.IsNullOrWhiteSpace(template.SubjectTemplate) ? null : template.SubjectTemplate,
            template.BodyTemplate,
            isEnabled,
            1,
            lastUpdated,
            template.AllowedMergeTags,
            new GovernanceNotificationChannelToggleResponse(
                trigger?.WhatsAppEnabled ?? string.Equals(template.Channel, "whatsapp", StringComparison.OrdinalIgnoreCase),
                trigger?.EmailEnabled ?? string.Equals(template.Channel, "email", StringComparison.OrdinalIgnoreCase),
                trigger?.SmsEnabled ?? string.Equals(template.Channel, "sms", StringComparison.OrdinalIgnoreCase),
                trigger?.PushEnabled ?? string.Equals(template.Channel, "push", StringComparison.OrdinalIgnoreCase)),
            [
                new GovernanceNotificationTemplateVersionResponse(
                    1,
                    lastUpdated,
                    "System")
            ]);
    }

    private static string NormalizeChannel(string channel)
    {
        return channel.Trim().ToLowerInvariant() switch
        {
            "whatsapp" => "whatsapp",
            "email" => "email",
            "sms" => "sms",
            "push" => "push",
            _ => "email",
        };
    }
}

public sealed record GovernanceNotificationTemplateResponse(
    string Id,
    string TriggerEvent,
    string Channel,
    string RecipientType,
    string? Subject,
    string Body,
    bool IsEnabled,
    int Version,
    DateTime LastUpdated,
    IReadOnlyCollection<string> MergeTags,
    GovernanceNotificationChannelToggleResponse ChannelToggles,
    IReadOnlyCollection<GovernanceNotificationTemplateVersionResponse> VersionHistory);

public sealed record GovernanceNotificationChannelToggleResponse(
    bool Whatsapp,
    bool Email,
    bool Sms,
    bool Push);

public sealed record GovernanceNotificationTemplateVersionResponse(
    int Version,
    DateTime UpdatedAt,
    string UpdatedBy);

public sealed record GovernanceNotificationSendLogResponse(
    string Id,
    string TriggerEvent,
    string Channel,
    string Recipient,
    string Status,
    DateTime SentAt,
    string? ErrorMessage);

public sealed record GovernanceNotificationTemplateUpdateRequest(
    string? Subject,
    string? Body,
    bool? IsEnabled,
    Dictionary<string, bool>? ChannelToggles,
    IReadOnlyCollection<string>? MergeTags);
