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
[Route("api/v{version:apiVersion}/refunds")]
public sealed class RefundController : ApiControllerBase
{
    private readonly ISender _sender;

    public RefundController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.PaymentCollect)]
    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<RefundDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> CreateRefundRequestAsync(
        [FromBody] CreateRefundRequestCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateRefundRequestCommand(
                request.CancellationRecordId,
                request.InvoiceId,
                request.RefundAmount,
                request.RefundMethod,
                request.RefundReason),
            cancellationToken);

        return Success(response, "Refund request created successfully.");
    }

    [Authorize(Policy = PermissionNames.PaymentRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<RefundListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<RefundListItemResponse>>>> GetRefundsAsync(
        [FromQuery] string? refundStatus,
        [FromQuery] long? customerId,
        [FromQuery] int? branchId,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetRefundRequestListQuery(refundStatus, customerId, branchId, fromDateUtc, toDateUtc),
            cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<RefundDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> GetRefundByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetRefundRequestDetailQuery(id), cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerRefundStatusResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerRefundStatusResponse>>>> GetCustomerRefundStatusAsync(
        [FromRoute] long customerId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetCustomerRefundStatusQuery(customerId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.PaymentCollect)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RefundRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundRequestResponse>>> InitiateRefundAsync(
        [FromBody] InitiateRefundRequest request,
        CancellationToken cancellationToken)
    {
        var detail = await _sender.Send(
            new CreateRefundRequestCommand(
                request.CancellationRecordId,
                request.InvoiceId,
                request.RequestedAmount,
                "OriginalPaymentMethod",
                request.Reason),
            cancellationToken);
        var response = new RefundRequestResponse(
            detail.RefundRequestId,
            detail.CancellationRecordId ?? 0,
            detail.InvoiceId ?? 0,
            detail.RefundStatus,
            detail.RequestedAmount,
            detail.ApprovedAmount);

        return Success(response, "Refund initiated successfully.");
    }

    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    [HttpPost("{refundRequestId:long}/approve")]
    [ProducesResponseType(typeof(ApiResponse<RefundDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> ApproveRefundAsync(
        [FromRoute] long refundRequestId,
        [FromBody] ApproveRefundRequestDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ApproveRefundRequestCommand(refundRequestId, request.ApprovedAmount, request.Remarks),
            cancellationToken);

        return Success(response, "Refund approved successfully.");
    }

    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    [HttpPost("{refundRequestId:long}/reject")]
    [ProducesResponseType(typeof(ApiResponse<RefundDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> RejectRefundAsync(
        [FromRoute] long refundRequestId,
        [FromBody] RejectRefundRequestDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RejectRefundRequestCommand(refundRequestId, request.Remarks),
            cancellationToken);

        return Success(response, "Refund rejected successfully.");
    }

    [Authorize(Policy = PermissionNames.ConfigurationManage)]
    [HttpPost("{refundRequestId:long}/status")]
    [ProducesResponseType(typeof(ApiResponse<RefundDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> UpdateRefundStatusAsync(
        [FromRoute] long refundRequestId,
        [FromBody] UpdateRefundStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateRefundStatusCommand(refundRequestId, request.RefundStatus, request.Remarks),
            cancellationToken);

        return Success(response, "Refund status updated successfully.");
    }
}
