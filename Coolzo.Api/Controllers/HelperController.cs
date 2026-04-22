using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseE.Helpers;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/helpers")]
public sealed class HelperController : ApiControllerBase
{
    private readonly ISender _sender;

    public HelperController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserCreate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HelperDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperDetailResponse>>> CreateAsync(
        [FromBody] CreateHelperProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateHelperProfileCommand(
                request.UserId,
                request.HelperCode,
                request.HelperName,
                request.MobileNo,
                request.ActiveFlag),
            cancellationToken);

        return Success(response, "Helper profile created successfully.");
    }

    [Authorize(Policy = PermissionNames.UserRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperListItemResponse>>>> GetListAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] int? branchId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHelperListQuery(searchTerm, branchId), cancellationToken);
        return Success(response);
    }

    [HttpGet("{helperProfileId:long}")]
    [ProducesResponseType(typeof(ApiResponse<HelperDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperDetailResponse>>> GetDetailAsync(
        [FromRoute] long helperProfileId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHelperDetailQuery(helperProfileId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPut("{helperProfileId:long}")]
    [ProducesResponseType(typeof(ApiResponse<HelperDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperDetailResponse>>> UpdateAsync(
        [FromRoute] long helperProfileId,
        [FromBody] UpdateHelperProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateHelperProfileCommand(
                helperProfileId,
                request.HelperCode,
                request.HelperName,
                request.MobileNo,
                request.ActiveFlag),
            cancellationToken);

        return Success(response, "Helper profile updated successfully.");
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("{helperProfileId:long}/assign")]
    [ProducesResponseType(typeof(ApiResponse<HelperDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperDetailResponse>>> AssignAsync(
        [FromRoute] long helperProfileId,
        [FromBody] AssignHelperToJobRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AssignHelperToJobCommand(
                helperProfileId,
                request.TechnicianId,
                request.ServiceRequestId,
                request.JobCardId,
                request.AssignmentRemarks),
            cancellationToken);

        return Success(response, "Helper assigned successfully.");
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("{helperProfileId:long}/release")]
    [ProducesResponseType(typeof(ApiResponse<HelperDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperDetailResponse>>> ReleaseAsync(
        [FromRoute] long helperProfileId,
        [FromBody] ReleaseHelperAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ReleaseHelperAssignmentCommand(helperProfileId, request.Remarks), cancellationToken);
        return Success(response, "Helper assignment released successfully.");
    }

    [HttpGet("{helperProfileId:long}/assignment")]
    [ProducesResponseType(typeof(ApiResponse<HelperAssignmentDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HelperAssignmentDetailResponse>>> GetAssignmentAsync(
        [FromRoute] long helperProfileId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHelperAssignmentDetailQuery(helperProfileId), cancellationToken);
        return Success(response);
    }
}
