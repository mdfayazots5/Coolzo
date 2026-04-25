using Coolzo.Application.Features.SystemConfiguration.Commands.CreateBusinessHourConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Commands.CreateHolidayConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Commands.CreateSystemConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Commands.UpdateSystemConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetBusinessHourConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetHolidayConfigurationList;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetSystemConfigurationDetail;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetSystemConfigurationList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Admin;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api")]
public sealed class SystemConfigurationController : ApiControllerBase
{
    private readonly ISender _sender;

    public SystemConfigurationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("system-configurations")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationResponse>>> CreateSystemConfigurationAsync(
        [FromBody] SystemConfigurationUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateSystemConfigurationCommand(
                request.ConfigurationGroup,
                request.ConfigurationKey,
                request.ConfigurationValue,
                request.ValueType,
                request.Description,
                request.IsSensitive,
                request.IsActive),
            cancellationToken);

        return Success(response, "System configuration created successfully.");
    }

    [HttpPut("system-configurations/{systemConfigurationId:long}")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationResponse>>> UpdateSystemConfigurationAsync(
        [FromRoute] long systemConfigurationId,
        [FromBody] SystemConfigurationUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateSystemConfigurationCommand(
                systemConfigurationId,
                request.ConfigurationGroup,
                request.ConfigurationKey,
                request.ConfigurationValue,
                request.ValueType,
                request.Description,
                request.IsSensitive,
                request.IsActive),
            cancellationToken);

        return Success(response, "System configuration updated successfully.");
    }

    [HttpGet("system-configurations")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SystemConfigurationResponse>>>> GetSystemConfigurationsAsync(
        [FromQuery] string? configurationGroup,
        [FromQuery] string? configurationKey,
        [FromQuery] string? valueType,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetSystemConfigurationListQuery(configurationGroup, configurationKey, valueType, isActive),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("system-configurations/{systemConfigurationId:long}")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationResponse>>> GetSystemConfigurationDetailAsync(
        [FromRoute] long systemConfigurationId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSystemConfigurationDetailQuery(systemConfigurationId), cancellationToken);

        return Success(response);
    }

    [HttpGet("business-hours")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>>> GetBusinessHoursAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetBusinessHourConfigurationQuery(), cancellationToken);

        return Success(response);
    }

    [HttpPost("business-hours")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>>> SaveBusinessHoursAsync(
        [FromBody] SaveBusinessHoursRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateBusinessHourConfigurationCommand(
                request.BusinessHours
                    .Select(item => new BusinessHourConfigurationInput(item.DayOfWeekNumber, item.StartTimeLocal, item.EndTimeLocal, item.IsClosed))
                    .ToArray()),
            cancellationToken);

        return Success(response, "Business hours updated successfully.");
    }

    [HttpGet("holidays")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HolidayConfigurationResponse>>>> GetHolidaysAsync(
        [FromQuery] int? year,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHolidayConfigurationListQuery(year, isActive), cancellationToken);

        return Success(response);
    }

    [HttpPost("holidays")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<HolidayConfigurationResponse>>> CreateHolidayAsync(
        [FromBody] CreateHolidayConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateHolidayConfigurationCommand(request.HolidayDate, request.HolidayName, request.IsRecurringAnnually, request.IsActive),
            cancellationToken);

        return Success(response, "Holiday created successfully.");
    }
}
