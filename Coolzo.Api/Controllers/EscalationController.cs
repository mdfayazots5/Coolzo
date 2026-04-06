using Asp.Versioning;
using Coolzo.Application.Features.GapPhaseA.Escalation;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize(Policy = PermissionNames.SupportManage)]
[Route("api/v{version:apiVersion}/escalations")]
public sealed class EscalationController : ApiControllerBase
{
    private readonly ISender _sender;

    public EscalationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EscalationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EscalationResponse>>> CreateAsync(
        [FromBody] CreateEscalationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateEscalationCommand(
                request.AlertType,
                request.RelatedEntityName,
                request.RelatedEntityId,
                request.Severity,
                request.EscalationLevel,
                request.SlaMinutes,
                request.NotificationChain,
                request.Message),
            cancellationToken);

        return Success(response, "Escalation created successfully.");
    }

    [HttpPost("service-requests/{serviceRequestId:long}/no-show")]
    [ProducesResponseType(typeof(ApiResponse<ServiceRequestDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceRequestDetailResponse>>> HandleNoShowAsync(
        [FromRoute] long serviceRequestId,
        [FromBody] HandleNoShowRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new HandleNoShowCommand(serviceRequestId, request.Reason, request.PreferredTechnicianId),
            cancellationToken);

        return Success(response, "No-show flow processed successfully.");
    }
}
