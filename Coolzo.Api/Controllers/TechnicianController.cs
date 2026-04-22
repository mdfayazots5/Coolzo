using Asp.Versioning;
using Coolzo.Application.Features.Technician.Management;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Technician;
using Coolzo.Contracts.Responses.Technician;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/technicians")]
public sealed class TechnicianController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>>> GetTechniciansAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool activeOnly = false,
        [FromQuery] string? zone = null,
        [FromQuery] string? skill = null,
        [FromQuery] string? availability = null,
        [FromQuery] decimal? minimumRating = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new SearchTechniciansQuery(searchTerm, activeOnly, zone, skill, availability, minimumRating),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("{technicianId:long}")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianDetailResponse>>> GetTechnicianAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTechnicianProfileQuery(technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserCreate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianDetailResponse>>> CreateTechnicianAsync(
        [FromBody] CreateTechnicianRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new CreateTechnicianProfileCommand(
                request.TechnicianName,
                request.MobileNumber,
                request.EmailAddress,
                request.BaseZoneId,
                request.MaxDailyAssignments,
                request.Skills,
                request.ZoneIds),
            cancellationToken);

        return Success(response, "Technician profile created successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPut("{technicianId:long}")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianDetailResponse>>> UpdateTechnicianAsync(
        [FromRoute] long technicianId,
        [FromBody] UpdateTechnicianRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new UpdateTechnicianProfileCommand(
                technicianId,
                request.TechnicianName,
                request.MobileNumber,
                request.EmailAddress,
                request.BaseZoneId,
                request.MaxDailyAssignments,
                request.IsActive),
            cancellationToken);

        return Success(response, "Technician profile updated successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("{technicianId:long}/performance")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianPerformanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianPerformanceResponse>>> GetPerformanceAsync(
        [FromRoute] long technicianId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTechnicianPerformanceQuery(technicianId, fromDate, toDate), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("{technicianId:long}/attendance")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianAttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianAttendanceResponse>>>> GetAttendanceAsync(
        [FromRoute] long technicianId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var response = await _sender.Send(
            new GetTechnicianAttendanceQuery(technicianId, year ?? today.Year, month ?? today.Month),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{technicianId:long}/attendance/leave")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianAttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianAttendanceResponse>>> RequestLeaveAsync(
        [FromRoute] long technicianId,
        [FromBody] CreateTechnicianLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new RequestTechnicianLeaveCommand(technicianId, request.LeaveDate, request.LeaveReason),
            cancellationToken);

        return Success(response, "Technician leave request submitted successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPatch("{technicianId:long}/attendance/leave/{leaveRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianAttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianAttendanceResponse>>> ReviewLeaveAsync(
        [FromRoute] long technicianId,
        [FromRoute] long leaveRequestId,
        [FromBody] ReviewTechnicianLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new ReviewTechnicianLeaveCommand(technicianId, leaveRequestId, request.Decision, request.Remarks),
            cancellationToken);

        return Success(response, "Technician leave request updated successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("availability-board")]
    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>>> GetAvailabilityBoardAsync(
        [FromQuery] long? serviceRequestId,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTechnicianAvailabilityBoardQuery(serviceRequestId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("{technicianId:long}/gps-log")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianGpsLogResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianGpsLogResponse>>>> GetGpsLogAsync(
        [FromRoute] long technicianId,
        [FromQuery] DateOnly? trackingDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetTechnicianGpsLogQuery(technicianId, trackingDate ?? DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPatch("{technicianId:long}/skills")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianSkillResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianSkillResponse>>>> UpdateSkillsAsync(
        [FromRoute] long technicianId,
        [FromBody] UpdateTechnicianSkillsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new UpdateTechnicianSkillsCommand(technicianId, request.Skills), cancellationToken);
        return Success(response, "Technician skill tags updated successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPatch("{technicianId:long}/zones")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianZoneResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianZoneResponse>>>> UpdateZonesAsync(
        [FromRoute] long technicianId,
        [FromBody] UpdateTechnicianZonesRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new UpdateTechnicianZonesCommand(technicianId, request.ZoneIds, request.PrimaryZoneId),
            cancellationToken);

        return Success(response, "Technician zone assignments updated successfully.");
    }
}
