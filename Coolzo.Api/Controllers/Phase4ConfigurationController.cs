using Asp.Versioning;
using Coolzo.Application.Features.MasterDataAdmin.Commands.CreateDynamicMasterRecord;
using Coolzo.Application.Features.MasterDataAdmin.Commands.DeleteDynamicMasterRecord;
using Coolzo.Application.Features.MasterDataAdmin.Commands.UpdateDynamicMasterRecord;
using Coolzo.Application.Features.MasterDataAdmin.Queries.GetDynamicMasterRecordList;
using Coolzo.Application.Features.SystemConfiguration.Commands.CreateBusinessHourConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Commands.CreateSystemConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Commands.UpdateSystemConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetBusinessHourConfiguration;
using Coolzo.Application.Features.SystemConfiguration.Queries.GetSystemConfigurationList;
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
[Route("api/v{version:apiVersion}")]
public sealed class Phase4ConfigurationController : ApiControllerBase
{
    private static readonly IReadOnlyDictionary<string, string> MasterTypeMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["service-types"] = "ServiceType",
            ["service-subtypes"] = "ServiceSubType",
            ["equipment-brands"] = "EquipmentBrand",
            ["equipment-models"] = "EquipmentModel",
            ["zones"] = "Zone",
            ["job-statuses"] = "JobStatus",
            ["urgency-levels"] = "UrgencyLevel",
            ["skill-tags"] = "SkillTag",
        };

    private static readonly IReadOnlyDictionary<string, string> ConfigurationGroupMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sla-targets"] = "SLATargets",
            ["slot-availability"] = "SlotAvailability",
            ["tax"] = "Tax",
            ["pricing-matrix"] = "PricingMatrix",
            ["amc-plans"] = "AMCPlans",
            ["payment-terms"] = "PaymentTerms",
            ["invoice-numbering"] = "InvoiceNumbering",
            ["warranty-periods"] = "WarrantyPeriods",
            ["auto-escalation-rules"] = "AutoEscalationRules",
        };

    private readonly ISender _sender;

    public Phase4ConfigurationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("master/{masterSlug}")]
    [Authorize(Policy = PermissionNames.LookupRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DynamicMasterRecordResponse>>>> GetMasterRecordsAsync(
        [FromRoute] string masterSlug,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var masterType = ResolveMasterType(masterSlug);
        var response = await _sender.Send(new GetDynamicMasterRecordListQuery(masterType, search, isActive), cancellationToken);

        return Success(response);
    }

    [HttpPost("master/{masterSlug}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<DynamicMasterRecordResponse>>> CreateMasterRecordAsync(
        [FromRoute] string masterSlug,
        [FromBody] MasterCatalogRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var masterType = ResolveMasterType(masterSlug);
        var response = await _sender.Send(
            new CreateDynamicMasterRecordCommand(
                masterType,
                request.MasterCode,
                request.MasterLabel,
                request.MasterValue,
                request.Description,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "Master data created successfully.");
    }

    [HttpPut("master/{masterSlug}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<DynamicMasterRecordResponse>>> UpdateMasterRecordAsync(
        [FromRoute] string masterSlug,
        [FromBody] MasterCatalogRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.DynamicMasterRecordId.HasValue)
        {
            return BadRequest("DynamicMasterRecordId is required.");
        }

        var masterType = ResolveMasterType(masterSlug);
        var response = await _sender.Send(
            new UpdateDynamicMasterRecordCommand(
                request.DynamicMasterRecordId.Value,
                masterType,
                request.MasterCode,
                request.MasterLabel,
                request.MasterValue,
                request.Description,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "Master data updated successfully.");
    }

    [HttpDelete("master/{masterSlug}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<DynamicMasterRecordResponse>>> DeleteMasterRecordAsync(
        [FromRoute] string masterSlug,
        [FromQuery] long dynamicMasterRecordId,
        CancellationToken cancellationToken)
    {
        _ = ResolveMasterType(masterSlug);
        var response = await _sender.Send(new DeleteDynamicMasterRecordCommand(dynamicMasterRecordId), cancellationToken);

        return Success(response, "Master data deleted successfully.");
    }

    [HttpGet("config/business-hours")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>>> GetBusinessHoursAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetBusinessHourConfigurationQuery(), cancellationToken);

        return Success(response);
    }

    [HttpPost("config/business-hours")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>>> CreateBusinessHoursAsync(
        [FromBody] SaveBusinessHoursRequest request,
        CancellationToken cancellationToken)
    {
        var response = await SaveBusinessHoursAsync(request, cancellationToken);
        return Success(response, "Business hours updated successfully.");
    }

    [HttpPut("config/business-hours")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>>> UpdateBusinessHoursAsync(
        [FromBody] SaveBusinessHoursRequest request,
        CancellationToken cancellationToken)
    {
        var response = await SaveBusinessHoursAsync(request, cancellationToken);
        return Success(response, "Business hours updated successfully.");
    }

    [HttpGet("config/{configSlug}")]
    [Authorize(Policy = PermissionNames.ConfigurationRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SystemConfigurationResponse>>>> GetConfigurationGroupAsync(
        [FromRoute] string configSlug,
        CancellationToken cancellationToken)
    {
        var configurationGroup = ResolveConfigurationGroup(configSlug);
        var response = await _sender.Send(new GetSystemConfigurationListQuery(configurationGroup, null, null, null), cancellationToken);

        return Success(response);
    }

    [HttpPost("config/{configSlug}")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationResponse>>> CreateConfigurationRecordAsync(
        [FromRoute] string configSlug,
        [FromBody] ConfigurationRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var configurationGroup = ResolveConfigurationGroup(configSlug);
        var response = await _sender.Send(
            new CreateSystemConfigurationCommand(
                configurationGroup,
                request.ConfigurationKey,
                request.ConfigurationValue,
                request.ValueType,
                request.Description,
                request.IsSensitive,
                request.IsActive),
            cancellationToken);

        return Success(response, "Configuration created successfully.");
    }

    [HttpPut("config/{configSlug}")]
    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationResponse>>> UpdateConfigurationRecordAsync(
        [FromRoute] string configSlug,
        [FromBody] ConfigurationRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.SystemConfigurationId.HasValue)
        {
            return BadRequest("SystemConfigurationId is required.");
        }

        var configurationGroup = ResolveConfigurationGroup(configSlug);
        var response = await _sender.Send(
            new UpdateSystemConfigurationCommand(
                request.SystemConfigurationId.Value,
                configurationGroup,
                request.ConfigurationKey,
                request.ConfigurationValue,
                request.ValueType,
                request.Description,
                request.IsSensitive,
                request.IsActive),
            cancellationToken);

        return Success(response, "Configuration updated successfully.");
    }

    private async Task<IReadOnlyCollection<BusinessHourConfigurationResponse>> SaveBusinessHoursAsync(
        SaveBusinessHoursRequest request,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(
            new CreateBusinessHourConfigurationCommand(
                request.BusinessHours
                    .Select(item => new BusinessHourConfigurationInput(item.DayOfWeekNumber, item.StartTimeLocal, item.EndTimeLocal, item.IsClosed))
                    .ToArray()),
            cancellationToken);
    }

    private static string ResolveMasterType(string masterSlug)
    {
        if (MasterTypeMap.TryGetValue(masterSlug, out var masterType))
        {
            return masterType;
        }

        throw new KeyNotFoundException($"Unsupported master slug '{masterSlug}'.");
    }

    private static string ResolveConfigurationGroup(string configSlug)
    {
        if (ConfigurationGroupMap.TryGetValue(configSlug, out var configurationGroup))
        {
            return configurationGroup;
        }

        throw new KeyNotFoundException($"Unsupported configuration slug '{configSlug}'.");
    }
}
