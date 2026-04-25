using Coolzo.Application.Features.GapPhaseE.Helpers;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/helpers/{helperProfileId:long}/attendance")]
public sealed class HelperAttendanceController : ApiControllerBase
{
    private readonly ISender _sender;

    public HelperAttendanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>>> CheckInAsync(
        [FromRoute] long helperProfileId,
        [FromBody] CheckInHelperAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CheckInHelperAttendanceCommand(helperProfileId, request.LocationText), cancellationToken);
        return Success(response, "Helper attendance check-in recorded successfully.");
    }

    [HttpPost("check-out")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>>> CheckOutAsync(
        [FromRoute] long helperProfileId,
        [FromBody] CheckOutHelperAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CheckOutHelperAttendanceCommand(helperProfileId, request.LocationText), cancellationToken);
        return Success(response, "Helper attendance check-out recorded successfully.");
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>>> GetListAsync(
        [FromRoute] long helperProfileId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetHelperAttendanceListQuery(helperProfileId), cancellationToken);
        return Success(response);
    }
}
