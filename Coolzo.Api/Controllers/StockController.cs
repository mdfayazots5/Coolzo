using Coolzo.Application.Features.Inventory.Commands.RecordStockTransaction;
using Coolzo.Application.Features.Inventory.Commands.TransferStock;
using Coolzo.Application.Features.Inventory.Queries.GetStockTransactions;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Inventory;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Route("api/stock")]
public sealed class StockController : ApiControllerBase
{
    private readonly ISender _sender;

    public StockController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.StockManage)]
    [HttpPost("transaction")]
    [ProducesResponseType(typeof(ApiResponse<StockTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StockTransactionResponse>>> RecordTransactionAsync(
        [FromBody] RecordStockTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new RecordStockTransactionCommand(
                request.WarehouseId,
                request.ItemId,
                request.TransactionType,
                request.Quantity,
                request.UnitCost,
                request.SupplierId,
                request.ReferenceNumber,
                request.Remarks),
            cancellationToken);

        return Success(response, "Stock transaction recorded successfully.");
    }

    [Authorize(Policy = PermissionNames.StockManage)]
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<StockTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<StockTransactionResponse>>>> TransferAsync(
        [FromBody] TransferStockRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new TransferStockCommand(
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.ItemId,
                request.Quantity,
                request.UnitCost,
                request.ReferenceNumber,
                request.Remarks),
            cancellationToken);

        return Success(response, "Stock transferred successfully.");
    }

    [Authorize(Policy = PermissionNames.StockRead)]
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<StockTransactionResponse>>>> GetTransactionsAsync(
        [FromQuery] string? transactionType,
        [FromQuery] long? itemId,
        [FromQuery] long? warehouseId,
        [FromQuery] long? technicianId,
        [FromQuery] long? jobCardId,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetStockTransactionsQuery(
                transactionType,
                itemId,
                warehouseId,
                technicianId,
                jobCardId,
                fromDateUtc,
                toDateUtc,
                pageNumber,
                pageSize),
            cancellationToken);

        return Success(response);
    }
}
