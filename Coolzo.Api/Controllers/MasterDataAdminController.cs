using Asp.Versioning;
using Coolzo.Application.Features.MasterDataAdmin.Commands.CreateDynamicMasterRecord;
using Coolzo.Application.Features.MasterDataAdmin.Commands.UpdateDynamicMasterRecord;
using Coolzo.Application.Features.MasterDataAdmin.Queries.GetDynamicMasterRecordList;
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
[Route("api/v{version:apiVersion}/admin-masters")]
public sealed class MasterDataAdminController : ApiControllerBase
{
    private readonly ISender _sender;

    public MasterDataAdminController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.LookupRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DynamicMasterRecordResponse>>>> GetAsync(
        [FromQuery] string? masterType,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDynamicMasterRecordListQuery(masterType, search, isActive), cancellationToken);

        return Success(response);
    }

    [HttpGet("{type}")]
    [Authorize(Policy = PermissionNames.LookupRead)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DynamicMasterRecordResponse>>>> GetByTypeAsync(
        [FromRoute] string type,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDynamicMasterRecordListQuery(type, search, isActive), cancellationToken);

        return Success(response);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<DynamicMasterRecordResponse>>> CreateAsync(
        [FromBody] DynamicMasterRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateDynamicMasterRecordCommand(
                request.MasterType,
                request.MasterCode,
                request.MasterLabel,
                request.MasterValue,
                request.Description,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "Dynamic master created successfully.");
    }

    [HttpPut("{dynamicMasterRecordId:long}")]
    [Authorize(Policy = PermissionNames.LookupManage)]
    public async Task<ActionResult<ApiResponse<DynamicMasterRecordResponse>>> UpdateAsync(
        [FromRoute] long dynamicMasterRecordId,
        [FromBody] DynamicMasterRecordUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateDynamicMasterRecordCommand(
                dynamicMasterRecordId,
                request.MasterType,
                request.MasterCode,
                request.MasterLabel,
                request.MasterValue,
                request.Description,
                request.IsActive,
                request.IsPublished,
                request.SortOrder),
            cancellationToken);

        return Success(response, "Dynamic master updated successfully.");
    }
}
