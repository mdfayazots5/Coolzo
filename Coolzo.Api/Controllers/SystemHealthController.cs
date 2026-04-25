using Coolzo.Application.Features.GapPhaseA.SystemHealth;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Policy = PermissionNames.HealthRead)]
[Route("api/system-health")]
public sealed class SystemHealthController : ApiControllerBase
{
    private readonly ISender _sender;

    public SystemHealthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SystemHealthResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSystemHealthSnapshotQuery(), cancellationToken);

        return Success(response);
    }
}
