using Coolzo.Application.Features.GapPhaseA.PartsReturn;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Policy = PermissionNames.StockManage)]
[Route("api/parts-returns")]
public sealed class PartsReturnController : ApiControllerBase
{
    private readonly ISender _sender;

    public PartsReturnController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PartsReturnResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PartsReturnResponse>>> CreateAsync(
        [FromBody] CreatePartsReturnRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreatePartsReturnCommand(
                request.ItemId,
                request.Quantity,
                request.ReasonCode,
                request.DefectDescription,
                request.TechnicianId,
                request.JobCardId),
            cancellationToken);

        return Success(response, "Parts return created successfully.");
    }

    [HttpPost("{partsReturnId:long}/approve")]
    [ProducesResponseType(typeof(ApiResponse<PartsReturnResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PartsReturnResponse>>> ApproveAsync(
        [FromRoute] long partsReturnId,
        [FromBody] ApprovePartsReturnRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ApprovePartsReturnCommand(partsReturnId, request.Remarks), cancellationToken);

        return Success(response, "Parts return approved successfully.");
    }
}
