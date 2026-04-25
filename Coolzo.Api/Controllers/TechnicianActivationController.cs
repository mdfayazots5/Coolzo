using Coolzo.Application.Features.GapPhaseE.TechnicianOnboarding;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/technicians/{technicianId:long}")]
public sealed class TechnicianActivationController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianActivationController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("activate")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingDetailResponse>>> ActivateAsync(
        [FromRoute] long technicianId,
        [FromBody] ActivateTechnicianPhaseERequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ActivateTechnicianPhaseECommand(technicianId, request.ActivationReason, null), cancellationToken);
        return Success(response, "Technician activated successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("deactivate")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingDetailResponse>>> DeactivateAsync(
        [FromRoute] long technicianId,
        [FromBody] DeactivateTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new DeactivateTechnicianCommand(technicianId, request.ActivationReason), cancellationToken);
        return Success(response, "Technician deactivated successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("activation-history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianActivationLogResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianActivationLogResponse>>>> GetHistoryAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianActivationHistoryQuery(technicianId), cancellationToken);
        return Success(response);
    }
}
