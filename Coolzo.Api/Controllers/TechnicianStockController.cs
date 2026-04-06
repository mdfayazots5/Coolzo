using Asp.Versioning;
using Coolzo.Application.Features.Inventory.Commands.AssignStockToTechnician;
using Coolzo.Application.Features.Inventory.Queries.GetTechnicianStock;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Inventory;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/technicians")]
public sealed class TechnicianStockController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianStockController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.StockManage)]
    [HttpPost("{id:long}/stock-assign")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<StockTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<StockTransactionResponse>>>> AssignAsync(
        [FromRoute] long id,
        [FromBody] AssignStockToTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new AssignStockToTechnicianCommand(
                id,
                request.SourceWarehouseId,
                request.ItemId,
                request.Quantity,
                request.UnitCost,
                request.ReferenceNumber,
                request.Remarks),
            cancellationToken);

        return Success(response, "Stock assigned to technician successfully.");
    }

    [Authorize(Policy = PermissionNames.StockRead)]
    [HttpGet("{id:long}/stock")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianStockResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianStockResponse>>> GetTechnicianStockAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianStockQuery(id), cancellationToken);

        return Success(response);
    }
}
