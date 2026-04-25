using Coolzo.Application.Features.GapPhaseA.CancellationRefund;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseD;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/customer-absent")]
public sealed class CustomerAbsentController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerAbsentController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = RoleNames.Technician)]
    [HttpPost("{serviceRequestId:long}/mark")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAbsentDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAbsentDetailResponse>>> MarkAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] MarkCustomerAbsentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new MarkCustomerAbsentCommand(
                serviceRequestId,
                request.AbsentReasonCode,
                request.AbsentReasonText,
                request.AttemptCount,
                request.ContactAttemptLog),
            cancellationToken);

        return Success(response, "Customer absent marked successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("{serviceRequestId:long}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAbsentDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAbsentDetailResponse>>> RescheduleAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] ResolveCustomerAbsentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RescheduleCustomerAbsentServiceRequestCommand(serviceRequestId, request.Remarks),
            cancellationToken);

        return Success(response, "Customer absent service request rescheduled successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("{serviceRequestId:long}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<CancellationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationDetailResponse>>> CancelAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] CancelCustomerAbsentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CancelCustomerAbsentServiceRequestCommand(
                serviceRequestId,
                request.CancellationReasonCode,
                request.CancellationReasonText,
                request.Remarks),
            cancellationToken);

        return Success(response, "Customer absent service request cancelled successfully.");
    }

    [Authorize]
    [HttpGet("{serviceRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerAbsentDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerAbsentDetailResponse>>> GetByServiceRequestIdAsync(
        [FromRoute] long serviceRequestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerAbsentDetailQuery(serviceRequestId), cancellationToken);

        return Success(response);
    }
}
