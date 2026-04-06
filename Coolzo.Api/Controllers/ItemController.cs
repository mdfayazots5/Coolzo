using Asp.Versioning;
using Coolzo.Application.Features.Inventory.Commands.CreateItem;
using Coolzo.Application.Features.Inventory.Commands.UpdateItem;
using Coolzo.Application.Features.Inventory.Queries.GetItemById;
using Coolzo.Application.Features.Inventory.Queries.GetItems;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Inventory;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/items")]
public sealed class ItemController : ApiControllerBase
{
    private readonly ISender _sender;

    public ItemController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.ItemCreate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> CreateAsync(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateItemCommand(
                request.CategoryCode,
                request.CategoryName,
                request.UnitOfMeasureCode,
                request.UnitOfMeasureName,
                request.SupplierCode,
                request.SupplierName,
                request.ItemCode,
                request.ItemName,
                request.ItemDescription,
                request.PurchasePrice,
                request.SellingPrice,
                request.TaxPercentage,
                request.WarrantyDays,
                request.ReorderLevel,
                request.IsActive),
            cancellationToken);

        return Success(response, "Inventory item created successfully.");
    }

    [Authorize(Policy = PermissionNames.ItemCreate)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> UpdateAsync(
        [FromRoute] long id,
        [FromBody] UpdateItemRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateItemCommand(
                id,
                request.CategoryCode,
                request.CategoryName,
                request.UnitOfMeasureCode,
                request.UnitOfMeasureName,
                request.SupplierCode,
                request.SupplierName,
                request.ItemCode,
                request.ItemName,
                request.ItemDescription,
                request.PurchasePrice,
                request.SellingPrice,
                request.TaxPercentage,
                request.WarrantyDays,
                request.ReorderLevel,
                request.IsActive),
            cancellationToken);

        return Success(response, "Inventory item updated successfully.");
    }

    [Authorize(Policy = PermissionNames.ItemRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ItemResponse>>>> GetItemsAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetItemsQuery(searchTerm, isActive, pageNumber, pageSize),
            cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.ItemRead)]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> GetByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetItemByIdQuery(id), cancellationToken);

        return Success(response);
    }
}
