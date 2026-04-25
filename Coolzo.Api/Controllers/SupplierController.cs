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
[Route("api/suppliers")]
public sealed class SupplierController : ApiControllerBase
{
    private readonly ISender _sender;

    public SupplierController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("claims")]
    [ProducesResponseType(typeof(ApiResponse<SupplierClaimResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupplierClaimResponse>>> CreateClaimAsync(
        [FromBody] CreateSupplierClaimRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateSupplierClaimCommand(request.PartsReturnId, request.SupplierClaimReference, request.Remarks),
            cancellationToken);

        return Success(response, "Supplier claim created successfully.");
    }
}
