using Coolzo.Application.Features.GapPhaseC.Installation;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseC;
using Coolzo.Contracts.Responses.GapPhaseC;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/installations/{installationId:long}")]
public sealed class InstallationExecutionController : ApiControllerBase
{
    private readonly ISender _sender;

    public InstallationExecutionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("create-order")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> CreateOrderAsync(
        [FromRoute] long installationId,
        [FromBody] CreateInstallationExecutionOrderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateInstallationExecutionOrderCommand(
                installationId,
                request.TechnicianId,
                request.ScheduledInstallationDateUtc,
                request.HelperCount,
                request.ExecutionRemarks),
            cancellationToken);

        return Success(response, "Installation order created successfully.");
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> StartInstallationAsync(
        [FromRoute] long installationId,
        [FromBody] StartInstallationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new StartInstallationCommand(installationId, request.Remarks), cancellationToken);

        return Success(response, "Installation started successfully.");
    }

    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> CompleteInstallationAsync(
        [FromRoute] long installationId,
        [FromBody] CompleteInstallationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CompleteInstallationCommand(installationId, request.WorkSummary), cancellationToken);

        return Success(response, "Installation completed successfully.");
    }

    [HttpPost("checklist")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> SaveChecklistAsync(
        [FromRoute] long installationId,
        [FromBody] SaveInstallationChecklistRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SaveInstallationChecklistCommand(installationId, request.Items), cancellationToken);

        return Success(response, "Installation checklist saved successfully.");
    }

    [HttpPost("commission")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> CommissionAsync(
        [FromRoute] long installationId,
        [FromBody] GenerateInstallationCommissioningRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GenerateInstallationCommissioningCommand(
                installationId,
                request.CustomerConfirmationName,
                request.CustomerSignatureName,
                request.ChecklistJson,
                request.Remarks,
                request.IsAccepted),
            cancellationToken);

        return Success(response, "Installation commissioning completed successfully.");
    }
}
