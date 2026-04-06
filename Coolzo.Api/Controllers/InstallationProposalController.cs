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
public sealed class InstallationProposalController : ApiControllerBase
{
    private readonly ISender _sender;

    public InstallationProposalController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("proposal")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> CreateProposalAsync(
        [FromRoute] long installationId,
        [FromBody] CreateInstallationProposalRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateInstallationProposalCommand(installationId, request.ProposalRemarks, request.Lines),
            cancellationToken);

        return Success(response, "Installation proposal created successfully.");
    }

    [HttpPost("proposal/approve")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> ApproveProposalAsync(
        [FromRoute] long installationId,
        [FromBody] ApproveInstallationProposalRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ApproveInstallationProposalCommand(installationId, request.CustomerRemarks), cancellationToken);

        return Success(response, "Installation proposal approved successfully.");
    }

    [HttpPost("proposal/reject")]
    [ProducesResponseType(typeof(ApiResponse<InstallationSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InstallationSummaryResponse>>> RejectProposalAsync(
        [FromRoute] long installationId,
        [FromBody] RejectInstallationProposalRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RejectInstallationProposalCommand(installationId, request.CustomerRemarks), cancellationToken);

        return Success(response, "Installation proposal rejected successfully.");
    }
}
