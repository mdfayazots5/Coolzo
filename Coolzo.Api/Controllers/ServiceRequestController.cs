using Coolzo.Application.Features.OperationsDashboard.Queries.GetDashboardSummary;
using Coolzo.Application.Features.ServiceRequest.Commands.CreateServiceRequestFromBooking;
using Coolzo.Application.Features.ServiceRequest.Commands.SaveServiceRequestNote;
using Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestStatus;
using Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestDetail;
using Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestList;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Requests.Operations;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/service-requests")]
public sealed class ServiceRequestController : ApiControllerBase
{
    private readonly ISender _sender;

    public ServiceRequestController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.ServiceRequestCreate)]
    [HttpPost("from-booking/{bookingId:long}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> CreateFromBookingAsync(
        [FromRoute] long bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CreateServiceRequestFromBookingCommand(bookingId), cancellationToken);

        return Success(response, "Service request created successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ServiceRequestListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ServiceRequestListItemResponse>>>> GetServiceRequestsAsync(
        [FromQuery] long? bookingId,
        [FromQuery] long? serviceId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? slotDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetServiceRequestListQuery(bookingId, serviceId, status, slotDate, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.OperationsDashboardRead)]
    [HttpGet("dashboard-summary")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsDashboardSummaryResponse>>> GetDashboardSummaryAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDashboardSummaryQuery(), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("{serviceRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> GetServiceRequestByIdAsync(
        [FromRoute] long serviceRequestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetServiceRequestDetailQuery(serviceRequestId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("{serviceRequestId:long}/notes")]
    [ProducesResponseType(typeof(ApiResponse<JobExecutionNoteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobExecutionNoteResponse>>> SaveNoteAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] SaveJobExecutionNoteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveServiceRequestNoteCommand(serviceRequestId, request.NoteText, request.IsCustomerVisible),
            cancellationToken);

        return Success(response, "Service request note saved successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("{serviceRequestId:long}/status")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> UpdateStatusAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] UpdateServiceRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateServiceRequestStatusCommand(serviceRequestId, request.Status, request.Remarks),
            cancellationToken);

        return Success(response, "Service request status updated successfully.");
    }
}
