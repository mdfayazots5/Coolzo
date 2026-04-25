using Coolzo.Application.Features.Assignment.Commands.AssignTechnician;
using Coolzo.Application.Features.Assignment.Commands.ReassignTechnician;
using Coolzo.Application.Features.Assignment.Queries.GetAssignmentHistory;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Operations;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/service-requests")]
public sealed class AssignmentController : ApiControllerBase
{
    private readonly ISender _sender;

    public AssignmentController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("{serviceRequestId:long}/assign")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> AssignAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AssignTechnicianCommand(serviceRequestId, request.TechnicianId, request.Remarks),
            cancellationToken);

        return Success(response, "Technician assigned successfully.");
    }

    [Authorize(Policy = PermissionNames.AssignmentManage)]
    [HttpPost("{serviceRequestId:long}/reassign")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> ReassignAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] ReassignTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ReassignTechnicianCommand(serviceRequestId, request.TechnicianId, request.Remarks),
            cancellationToken);

        return Success(response, "Technician reassigned successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestRead)]
    [HttpGet("{serviceRequestId:long}/assignment-history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AssignmentHistoryItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AssignmentHistoryItemResponse>>>> GetAssignmentHistoryAsync(
        [FromRoute] long serviceRequestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAssignmentHistoryQuery(serviceRequestId), cancellationToken);

        return Success(response);
    }
}
