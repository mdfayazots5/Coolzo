using Coolzo.Application.Features.Scheduling;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Operations;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/scheduling")]
public sealed class SchedulingBoardController : ApiControllerBase
{
    private readonly ISender _sender;

    public SchedulingBoardController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("board")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingBoardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingBoardResponse>>> GetBoardAsync(
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        [FromQuery] long? technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSchedulingBoardQuery(dateFrom, dateTo, technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingBoardJobResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingBoardJobResponse>>> AssignAsync(
        [FromBody] ScheduleAssignServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ScheduleAssignServiceRequestCommand(
                request.ServiceRequestId,
                request.TechnicianId,
                request.SlotAvailabilityId,
                request.Remarks),
            cancellationToken);

        return Success(response, "Service request scheduled successfully.");
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPut("reassign")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingBoardJobResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingBoardJobResponse>>> ReassignAsync(
        [FromBody] ScheduleReassignServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ScheduleReassignServiceRequestCommand(
                request.ServiceRequestId,
                request.TechnicianId,
                request.SlotAvailabilityId,
                request.Remarks),
            cancellationToken);

        return Success(response, "Scheduling change saved successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("amc-auto")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchedulingAmcAutoVisitResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SchedulingAmcAutoVisitResponse>>>> GetAmcAutoAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSchedulingAmcAutoQuery(dateFrom, dateTo), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("amc-bulk-assign")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchedulingBoardJobResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SchedulingBoardJobResponse>>>> BulkAssignAmcAsync(
        [FromBody] ScheduleAmcBulkAssignRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ScheduleAmcBulkAssignCommand(request.TechnicianId, request.Visits, request.Remarks),
            cancellationToken);

        return Success(response, "AMC visits scheduled successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("conflicts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchedulingConflictResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SchedulingConflictResponse>>>> GetConflictsAsync(
        [FromQuery] long serviceRequestId,
        [FromQuery] long technicianId,
        [FromQuery] long slotAvailabilityId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetSchedulingConflictsQuery(serviceRequestId, technicianId, slotAvailabilityId),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("slots")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchedulingSlotResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SchedulingSlotResponse>>>> GetSlotsAsync(
        [FromQuery] long zoneId,
        [FromQuery] DateOnly slotDate,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSchedulingSlotsQuery(zoneId, slotDate), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPut("slots/{slotAvailabilityId:long}")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingSlotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingSlotResponse>>> UpdateSlotAsync(
        [FromRoute] long slotAvailabilityId,
        [FromBody] ScheduleUpdateSlotRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateScheduleSlotCommand(slotAvailabilityId, request.IsBlocked, request.AvailableCapacity),
            cancellationToken);

        return Success(response, "Scheduling slot updated successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("shifts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchedulingShiftResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SchedulingShiftResponse>>>> GetShiftsAsync(
        [FromQuery] long? technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianShiftsQuery(technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPut("shifts")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingShiftResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingShiftResponse>>> UpdateShiftsAsync(
        [FromBody] ScheduleUpdateTechnicianShiftsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateTechnicianShiftsCommand(request.TechnicianId, request.Days),
            cancellationToken);

        return Success(response, "Technician shifts updated successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("day-sheet")]
    [ProducesResponseType(typeof(ApiResponse<SchedulingDaySheetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SchedulingDaySheetResponse>>> GetDaySheetAsync(
        [FromQuery] DateOnly scheduleDate,
        [FromQuery] long? technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSchedulingDaySheetQuery(scheduleDate, technicianId), cancellationToken);
        return Success(response);
    }
}
