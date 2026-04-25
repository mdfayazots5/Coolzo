using Coolzo.Api.Extensions;
using Coolzo.Application.Features.FieldWorkflow;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldWorkflow;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Contracts.Responses.FieldWorkflow;
using Coolzo.Contracts.Responses.Technician;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Roles = RoleNames.Technician)]
[Route("api/field")]
public sealed class FieldWorkflowController : ApiControllerBase
{
    private readonly ISender _sender;

    public FieldWorkflowController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("my-jobs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianJobListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianJobListItemResponse>>>> GetMyJobsAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetFieldMyJobsQuery(), cancellationToken);
        return Success(response);
    }

    [HttpGet("job-history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianJobListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianJobListItemResponse>>>> GetJobHistoryAsync(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetFieldJobHistoryQuery(), cancellationToken);
        return Success(response);
    }

    [HttpGet("jobs/{serviceRequestId:long}")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobDetailResponse>>> GetJobDetailAsync(
        [FromRoute] long serviceRequestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetFieldJobDetailQuery(serviceRequestId), cancellationToken);
        return Success(response);
    }

    [HttpPatch("jobs/{serviceRequestId:long}/depart")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobDetailResponse>>> DepartAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new DepartFieldJobCommand(serviceRequestId, request.Latitude, request.Longitude, request.Remarks),
            cancellationToken);

        return Success(response, "Field job marked en route successfully.");
    }

    [HttpPatch("jobs/{serviceRequestId:long}/arrive")]
    [ProducesResponseType(typeof(ApiResponse<FieldArrivalValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FieldArrivalValidationResponse>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<FieldArrivalValidationResponse>>> ArriveAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ArriveFieldJobCommand(
                serviceRequestId,
                request.Latitude,
                request.Longitude,
                request.Remarks,
                request.OverrideReason),
            cancellationToken);

        if (response.OverrideRequired)
        {
            return UnprocessableEntity(
                ApiResponseFactory.Failure(
                    ErrorCodes.ValidationFailure,
                    response.Message,
                    HttpContext.TraceIdentifier,
                    Array.Empty<ApiError>(),
                    response));
        }

        return Ok(ApiResponseFactory.Success(response, HttpContext.TraceIdentifier, "Field arrival recorded successfully."));
    }

    [HttpPatch("jobs/{serviceRequestId:long}/start-work")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobDetailResponse>>> StartWorkAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new StartFieldJobWorkCommand(serviceRequestId, request.Remarks), cancellationToken);
        return Success(response, "Field work started successfully.");
    }

    [HttpPatch("jobs/{serviceRequestId:long}/progress")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobDetailResponse>>> SaveProgressAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobProgressRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SaveFieldJobProgressCommand(serviceRequestId, request.Items, request.Remarks), cancellationToken);
        return Success(response, "Field progress saved successfully.");
    }

    [HttpPost("jobs/{serviceRequestId:long}/parts-request")]
    [ProducesResponseType(typeof(ApiResponse<FieldPartsRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldPartsRequestResponse>>> CreatePartsRequestAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldPartsRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateFieldPartsRequestCommand(serviceRequestId, request.Urgency, request.Items, request.Notes),
            cancellationToken);

        return Success(response, "Field parts request submitted successfully.");
    }

    [HttpPost("jobs/{serviceRequestId:long}/estimate")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> CreateEstimateAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldEstimateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateFieldEstimateCommand(serviceRequestId, request.Lines, request.DiscountAmount, request.TaxPercentage, request.Remarks),
            cancellationToken);

        return Success(response, "Field estimate created successfully.");
    }

    [HttpPost("jobs/{serviceRequestId:long}/report")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobReportResponse>>> SubmitReportAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobReportRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitFieldJobReportCommand(
                serviceRequestId,
                request.EquipmentCondition,
                request.IssuesIdentified,
                request.ActionTaken,
                request.Recommendation,
                request.Observations,
                request.IdempotencyKey),
            cancellationToken);

        return Success(response, "Field job report submitted successfully.");
    }

    [HttpPost("jobs/{serviceRequestId:long}/photos")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobPhotoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobPhotoResponse>>> UploadPhotoAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobPhotoUploadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UploadFieldJobPhotoCommand(
                serviceRequestId,
                request.PhotoType,
                request.FileName,
                request.ContentType,
                request.Base64Content,
                request.Remarks),
            cancellationToken);

        return Success(response, "Field job photo uploaded successfully.");
    }

    [HttpPost("jobs/{serviceRequestId:long}/signature")]
    [ProducesResponseType(typeof(ApiResponse<FieldCustomerSignatureResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldCustomerSignatureResponse>>> SaveSignatureAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobSignatureRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveFieldJobSignatureCommand(serviceRequestId, request.CustomerName, request.SignatureBase64, request.Remarks),
            cancellationToken);

        return Success(response, "Field customer signature captured successfully.");
    }

    [HttpPatch("jobs/{serviceRequestId:long}/payment")]
    [ProducesResponseType(typeof(ApiResponse<PaymentTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentTransactionResponse>>> CollectPaymentAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CollectFieldJobPaymentCommand(
                serviceRequestId,
                request.PaidAmount,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Remarks,
                request.IdempotencyKey,
                request.GatewayTransactionId,
                request.Signature,
                request.ExpectedInvoiceAmount),
            cancellationToken);

        return Success(response, "Field payment recorded successfully.");
    }

    [HttpPatch("jobs/{serviceRequestId:long}/complete")]
    [ProducesResponseType(typeof(ApiResponse<FieldJobDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FieldJobDetailResponse>>> CompleteAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] FieldJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CompleteFieldJobCommand(serviceRequestId, request.Remarks), cancellationToken);
        return Success(response, "Field job completed successfully.");
    }

    [HttpPost("attendance/check-in")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianAttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianAttendanceResponse>>> CheckInAsync(
        [FromBody] FieldAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CheckInFieldAttendanceCommand(request.LocationText, request.Latitude, request.Longitude),
            cancellationToken);

        return Success(response, "Technician attendance check-in recorded successfully.");
    }

    [HttpPost("attendance/check-out")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianAttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianAttendanceResponse>>> CheckOutAsync(
        [FromBody] FieldAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CheckOutFieldAttendanceCommand(request.LocationText, request.Latitude, request.Longitude),
            cancellationToken);

        return Success(response, "Technician attendance check-out recorded successfully.");
    }
}
