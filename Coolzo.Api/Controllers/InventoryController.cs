using Coolzo.Application.Features.Inventory.Queries.GetItems;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouses;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/inventory")]
public sealed class InventoryController : ApiControllerBase
{
    private readonly ISender _sender;

    public InventoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<InventoryDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryDashboardResponse>>> GetDashboardAsync(
        CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetItemsQuery(null, true, 1, 200), cancellationToken);
        var primaryStock = await GetPrimaryWarehouseStockAsync(cancellationToken);
        var stockItems = primaryStock?.Items ?? Array.Empty<WarehouseStockItemResponse>();
        var purchasePriceByItemId = items.Items.ToDictionary(item => item.ItemId, item => item.PurchasePrice);

        var response = new InventoryDashboardResponse(
            items.TotalCount,
            stockItems.Sum(item => item.QuantityOnHand * purchasePriceByItemId.GetValueOrDefault(item.ItemId, 0.00m)),
            stockItems.Count(item => item.IsLowStock && item.QuantityOnHand > 0),
            stockItems.Count(item => item.QuantityOnHand <= 0),
            0,
            0);

        return Success(response);
    }

    [HttpGet("low-stock-alerts")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<InventoryLowStockAlertResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventoryLowStockAlertResponse>>>> GetLowStockAlertsAsync(
        CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetItemsQuery(null, true, 1, 200), cancellationToken);
        var itemLookup = items.Items.ToDictionary(item => item.ItemId);
        var primaryStock = await GetPrimaryWarehouseStockAsync(cancellationToken);

        var response = (primaryStock?.Items ?? Array.Empty<WarehouseStockItemResponse>())
            .Where(item => item.IsLowStock)
            .OrderBy(item => item.QuantityOnHand <= 0 ? 0 : 1)
            .ThenBy(item => item.QuantityOnHand)
            .ThenBy(item => item.ItemName)
            .Select(item =>
            {
                itemLookup.TryGetValue(item.ItemId, out var catalogItem);

                return new InventoryLowStockAlertResponse(
                    item.ItemId.ToString(),
                    item.ItemCode,
                    item.ItemName,
                    item.CategoryName,
                    catalogItem?.ItemDescription ?? string.Empty,
                    Array.Empty<string>(),
                    catalogItem?.PurchasePrice ?? 0.00m,
                    catalogItem?.SellingPrice,
                    item.QuantityOnHand,
                    item.ReorderLevel,
                    Math.Max(item.ReorderLevel - item.QuantityOnHand, 0),
                    "Main Warehouse",
                    item.QuantityOnHand <= 0 ? "out_of_stock" : "low_stock",
                    null,
                    catalogItem?.SupplierId.HasValue == true ? [catalogItem.SupplierId.Value.ToString()] : Array.Empty<string>());
            })
            .ToArray();

        return Success<IReadOnlyCollection<InventoryLowStockAlertResponse>>(response);
    }

    private async Task<WarehouseStockResponse?> GetPrimaryWarehouseStockAsync(CancellationToken cancellationToken)
    {
        var warehouses = await _sender.Send(new GetWarehousesQuery(null, true), cancellationToken);
        var primaryWarehouse = warehouses.OrderBy(item => item.WarehouseId).FirstOrDefault();

        if (primaryWarehouse is null)
        {
            return null;
        }

        return await _sender.Send(new GetWarehouseStockQuery(primaryWarehouse.WarehouseId), cancellationToken);
    }
}

public sealed record InventoryDashboardResponse(
    int TotalSKUs,
    decimal TotalStockValue,
    int LowStockCount,
    int OutOfStockCount,
    int PendingRequests,
    int OpenPOs);

public sealed record InventoryLowStockAlertResponse(
    string Id,
    string PartCode,
    string Name,
    string Category,
    string Description,
    IReadOnlyCollection<string> CompatibleBrands,
    decimal UnitCost,
    decimal? SellingPrice,
    decimal StockQuantity,
    decimal MinReorderLevel,
    decimal ReorderQuantity,
    string Location,
    string Status,
    string? ImageUrl,
    IReadOnlyCollection<string> SupplierIds);
