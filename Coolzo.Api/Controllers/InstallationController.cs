using Coolzo.Application.Features.GapPhaseC.Installation;
using Coolzo.Application.Features.GapPhaseA.Installation;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseC;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseC;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/installations")]
public sealed class InstallationController : ApiControllerBase
{
    private readonly ISender _sender;

    public InstallationController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> CreateInstallationAsync(
        [FromBody] CreateInstallationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateInstallationCommand(
                request.LeadId,
                request.CustomerName,
                request.MobileNumber,
                request.EmailAddress,
                request.SourceChannel,
                request.AddressLine1,
                request.AddressLine2,
                request.CityName,
                request.Pincode,
                request.InstallationType,
                request.NumberOfUnits,
                request.SiteNotes,
                request.PreferredSurveyDateUtc),
            cancellationToken);

        return Success(response, "Installation request created successfully.");
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InstallationListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InstallationListItemResponse>>>> GetInstallationsAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] string? installationStatus,
        [FromQuery] string? approvalStatus,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetInstallationListQuery(searchTerm, installationStatus, approvalStatus, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }

    [Authorize]
    [HttpGet("{installationId:long}")]
    [ProducesResponseType(typeof(ApiResponse<InstallationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationDetailResponse>>> GetInstallationDetailAsync(
        [FromRoute] long installationId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetInstallationDetailQuery(installationId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ServiceRequestUpdate)]
    [HttpPost("orders")]
    [ProducesResponseType(typeof(ApiResponse<InstallationOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationOrderResponse>>> CreateInstallationOrderAsync(
        [FromBody] CreateInstallationOrderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateInstallationOrderCommand(
                request.LeadId,
                request.ServiceRequestId,
                request.CustomerId,
                request.CustomerAddressId,
                request.TechnicianId,
                request.ScheduledInstallationDateUtc,
                request.InstallationChecklistJson),
            cancellationToken);

        return Success(response, "Installation order created successfully.");
    }

    [HttpPost("orders/{installationOrderId:long}/survey-report")]
    [ProducesResponseType(typeof(ApiResponse<InstallationOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationOrderResponse>>> SubmitSurveyReportAsync(
        [FromRoute] long installationOrderId,
        [FromBody] SubmitSurveyReportRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitSurveyReportCommand(
                installationOrderId,
                request.SurveyDecision,
                request.SiteConditionSummary,
                request.ElectricalReadiness,
                request.AccessReadiness,
                request.SafetyRiskNotes,
                request.RecommendedAction,
                request.EstimatedMaterialCost,
                request.SyncDeviceReference,
                request.SyncReference),
            cancellationToken);

        return Success(response, "Survey report submitted successfully.");
    }

    [HttpPost("orders/{installationOrderId:long}/commissioning-certificate")]
    [ProducesResponseType(typeof(ApiResponse<CommissioningCertificateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CommissioningCertificateResponse>>> CreateCommissioningCertificateAsync(
        [FromRoute] long installationOrderId,
        [FromBody] CreateCommissioningCertificateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateCommissioningCertificateCommand(
                installationOrderId,
                request.CustomerConfirmationName,
                request.ChecklistJson,
                request.Remarks,
                request.IsAccepted),
            cancellationToken);

        return Success(response, "Commissioning certificate created successfully.");
    }
}
