using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseC.Installation;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseC;
using Coolzo.Contracts.Responses.GapPhaseC;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/installations/{installationId:long}")]
public sealed class InstallationSurveyController : ApiControllerBase
{
    private readonly ISender _sender;

    public InstallationSurveyController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("schedule-survey")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> ScheduleSurveyAsync(
        [FromRoute] long installationId,
        [FromBody] ScheduleInstallationSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ScheduleInstallationSurveyCommand(installationId, request.SurveyDateUtc, request.TechnicianId, request.Remarks),
            cancellationToken);

        return Success(response, "Installation survey scheduled successfully.");
    }

    [HttpPost("submit-survey")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> SubmitSurveyAsync(
        [FromRoute] long installationId,
        [FromBody] SubmitInstallationSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitInstallationSurveyCommand(
                installationId,
                request.SiteConditionSummary,
                request.ElectricalReadiness,
                request.AccessReadiness,
                request.SafetyRiskNotes,
                request.RecommendedAction,
                request.EstimatedMaterialCost,
                request.MeasurementsJson,
                request.PhotoUrlsJson,
                request.Items),
            cancellationToken);

        return Success(response, "Installation survey submitted successfully.");
    }
}
