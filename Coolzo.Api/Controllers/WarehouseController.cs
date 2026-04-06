using Asp.Versioning;
using Coolzo.Application.Features.Inventory.Commands.CreateWarehouse;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouses;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Inventory;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/warehouses")]
public sealed class WarehouseController : ApiControllerBase
{
    private readonly ISender _sender;

    public WarehouseController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.WarehouseCreate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WarehouseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> CreateAsync(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateWarehouseCommand(
                request.WarehouseCode,
                request.WarehouseName,
                request.ContactPerson,
                request.MobileNumber,
                request.EmailAddress,
                request.AddressLine1,
                request.AddressLine2,
                request.Landmark,
                request.CityName,
                request.Pincode,
                request.IsActive),
            cancellationToken);

        return Success(response, "Warehouse created successfully.");
    }

    [Authorize(Policy = PermissionNames.WarehouseRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<WarehouseResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<WarehouseResponse>>>> GetWarehousesAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetWarehousesQuery(searchTerm, isActive), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.WarehouseRead)]
    [HttpGet("{id:long}/stock")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseStockResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WarehouseStockResponse>>> GetWarehouseStockAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetWarehouseStockQuery(id), cancellationToken);

        return Success(response);
    }
}
