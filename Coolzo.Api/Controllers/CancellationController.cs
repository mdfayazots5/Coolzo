using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseA.CancellationRefund;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Requests.GapPhaseD;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cancellations")]
public sealed class CancellationController : ApiControllerBase
{
    private readonly ISender _sender;

    public CancellationController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost("customer")]
    [ProducesResponseType(typeof(ApiResponse<CancellationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationDetailResponse>>> CreateCustomerCancellationAsync(
        [FromBody] CreateCustomerCancellationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCustomerCancellationCommand(
                request.BookingId,
                request.ServiceRequestId,
                request.CancellationReasonCode,
                request.CancellationReasonText),
            cancellationToken);

        return Success(response, "Customer cancellation created successfully.");
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("admin")]
    [ProducesResponseType(typeof(ApiResponse<CancellationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationDetailResponse>>> CreateAdminCancellationAsync(
        [FromBody] CreateAdminCancellationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateAdminCancellationCommand(
                request.BookingId,
                request.ServiceRequestId,
                request.CancellationSource,
                request.CancellationReasonCode,
                request.CancellationReasonText,
                request.ForceOverride,
                request.OverrideReason),
            cancellationToken);

        return Success(response, "Admin cancellation created successfully.");
    }

    [Authorize]
    [HttpGet("options/{serviceRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CancellationOptionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationOptionsResponse>>> GetCancellationOptionsAsync(
        [FromRoute] long serviceRequestId,
        [FromQuery] long? bookingId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCancellationOptionsQuery(serviceRequestId, bookingId), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CancellationListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CancellationListItemResponse>>>> GetCancellationsAsync(
        [FromQuery] long? bookingId,
        [FromQuery] long? serviceRequestId,
        [FromQuery] string? cancellationStatus,
        [FromQuery] string? cancellationSource,
        [FromQuery] string? cancellationReasonCode,
        [FromQuery] int? branchId,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetCancellationListQuery(
                bookingId,
                serviceRequestId,
                cancellationStatus,
                cancellationSource,
                cancellationReasonCode,
                branchId,
                fromDateUtc,
                toDateUtc),
            cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CancellationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationDetailResponse>>> GetCancellationByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCancellationDetailQuery(id), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("service-requests/{serviceRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<CancellationRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CancellationRecordResponse>>> CancelServiceRequestAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] CancelServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        var detail = await _sender.Send(
            new CreateAdminCancellationCommand(
                null,
                serviceRequestId,
                "Operations",
                request.ReasonCode,
                request.ReasonDescription,
                request.RequiresApproval,
                request.ReasonDescription),
            cancellationToken);
        var response = new CancellationRecordResponse(
            detail.CancellationRecordId,
            detail.ServiceRequestId ?? 0,
            detail.CancellationStatus,
            detail.CancellationFee,
            detail.RefundEligibleAmount,
            detail.ApprovalRequired);

        return Success(response, "Service request cancelled successfully.");
    }
}
