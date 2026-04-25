using Coolzo.Application.Features.Inventory.Queries.GetItems;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;
using Coolzo.Application.Features.Inventory.Queries.GetWarehouses;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/inventory")]
public sealed class InventoryController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly CoolzoDbContext _dbContext;

    public InventoryController(ISender sender, CoolzoDbContext dbContext)
    {
        _sender = sender;
        _dbContext = dbContext;
    }

    [HttpGet("parts")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<InventoryPartResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventoryPartResponse>>>> GetPartsAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var items = await BuildPartsQuery()
            .Where(item =>
                (string.IsNullOrWhiteSpace(searchTerm)
                    || item.ItemCode.Contains(searchTerm)
                    || item.ItemName.Contains(searchTerm)
                    || item.ItemDescription.Contains(searchTerm))
                && (!isActive.HasValue || item.IsActive == isActive.Value))
            .OrderBy(item => item.ItemName)
            .ToArrayAsync(cancellationToken);

        return Success<IReadOnlyCollection<InventoryPartResponse>>(items.Select(MapPart).ToArray());
    }

    [HttpGet("parts/{id:long}")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<InventoryPartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventoryPartResponse>>> GetPartByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var item = await BuildPartsQuery()
            .FirstOrDefaultAsync(entity =>
                !entity.IsDeleted &&
                entity.ItemId == id,
                cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Success(MapPart(item));
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

    [HttpGet("parts-requests")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<InventoryPartsRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventoryPartsRequestResponse>>>> GetPartsRequestsAsync(
        CancellationToken cancellationToken)
    {
        var requests = await _dbContext.PartsRequests
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.Technician)
            .Include(entity => entity.Items.Where(item => !item.IsDeleted))
            .Where(entity => !entity.IsDeleted)
            .OrderByDescending(entity => entity.Urgency)
            .ThenBy(entity => entity.SubmittedAtUtc)
            .ToArrayAsync(cancellationToken);

        var partIdLookup = await BuildPartIdLookupAsync(requests, cancellationToken);
        return Success<IReadOnlyCollection<InventoryPartsRequestResponse>>(requests.Select(request => MapPartsRequest(request, partIdLookup)).ToArray());
    }

    [HttpGet("parts-requests/{id:long}")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<InventoryPartsRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventoryPartsRequestResponse>>> GetPartsRequestByIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var request = await _dbContext.PartsRequests
            .AsNoTracking()
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.Technician)
            .Include(entity => entity.Items.Where(item => !item.IsDeleted))
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.PartsRequestId == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        var partIdLookup = await BuildPartIdLookupAsync([request], cancellationToken);
        return Success(MapPartsRequest(request, partIdLookup));
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

    private IQueryable<Item> BuildPartsQuery()
    {
        return _dbContext.Items
            .AsNoTracking()
            .Include(item => item.ItemCategory)
            .Include(item => item.Supplier)
            .Include(item => item.Rates.Where(rate => !rate.IsDeleted))
            .Include(item => item.WarehouseStocks.Where(stock => !stock.IsDeleted));
    }

    private async Task<Dictionary<string, long>> BuildPartIdLookupAsync(
        IEnumerable<PartsRequest> requests,
        CancellationToken cancellationToken)
    {
        var missingCodes = requests
            .SelectMany(request => request.Items)
            .Where(item => !item.IsDeleted && !item.ItemId.HasValue && !string.IsNullOrWhiteSpace(item.PartCode))
            .Select(item => item.PartCode.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingCodes.Length == 0)
        {
            return new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        }

        var records = await _dbContext.Items
            .AsNoTracking()
            .Where(item => !item.IsDeleted && missingCodes.Contains(item.ItemCode))
            .ToDictionaryAsync(item => item.ItemCode, item => item.ItemId, cancellationToken);

        return new Dictionary<string, long>(records, StringComparer.OrdinalIgnoreCase);
    }

    private static InventoryPartsRequestResponse MapPartsRequest(
        PartsRequest request,
        IReadOnlyDictionary<string, long> partIdLookup)
    {
        return new InventoryPartsRequestResponse(
            request.PartsRequestId.ToString(),
            request.TechnicianId.ToString(),
            request.Technician?.TechnicianName ?? $"Technician {request.TechnicianId}",
            request.ServiceRequestId.ToString(),
            request.ServiceRequest?.ServiceRequestNumber ?? $"SR-{request.ServiceRequestId}",
            MapUrgency(request.Urgency),
            MapPartsRequestStatus(request.CurrentStatus),
            request.Items
                .OrderBy(item => item.PartsRequestItemId)
                .Select(item => MapPartsRequestItem(item, partIdLookup))
                .ToArray(),
            request.SubmittedAtUtc,
            request.ProcessedAtUtc,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes);
    }

    private static InventoryPartsRequestItemResponse MapPartsRequestItem(
        PartsRequestItem item,
        IReadOnlyDictionary<string, long> partIdLookup)
    {
        var partId = item.ItemId
            ?? (partIdLookup.TryGetValue(item.PartCode, out var resolvedItemId) ? resolvedItemId : 0L);

        return new InventoryPartsRequestItemResponse(
            partId.ToString(),
            item.PartName,
            item.QuantityRequested,
            item.QuantityApproved > 0 ? item.QuantityApproved : null,
            item.QuantityApproved >= item.QuantityRequested ? "available" : item.QuantityApproved > 0 ? "insufficient" : "out_of_stock");
    }

    private static string MapUrgency(PartsRequestUrgency urgency)
    {
        return urgency == PartsRequestUrgency.Emergency ? "emergency" : "normal";
    }

    private static string MapPartsRequestStatus(PartsRequestStatus status)
    {
        return status switch
        {
            PartsRequestStatus.Approved => "approved",
            PartsRequestStatus.PartiallyApproved => "partially_approved",
            PartsRequestStatus.Rejected => "rejected",
            _ => "pending",
        };
    }

    private static InventoryPartResponse MapPart(Item item)
    {
        var stockQuantity = item.WarehouseStocks
            .Where(stock => !stock.IsDeleted)
            .Sum(stock => stock.QuantityOnHand);
        var unitRate = item.Rates
            .Where(rate => !rate.IsDeleted)
            .OrderByDescending(rate => rate.IsActive)
            .ThenByDescending(rate => rate.EffectiveFromUtc)
            .FirstOrDefault();

        return new InventoryPartResponse(
            item.ItemId.ToString(),
            item.ItemCode,
            item.ItemName,
            item.ItemCategory?.CategoryName ?? item.ItemCategoryId.ToString(),
            item.ItemDescription,
            Array.Empty<string>(),
            unitRate?.PurchasePrice ?? 0.00m,
            unitRate?.SellingPrice,
            stockQuantity,
            item.ReorderLevel,
            Math.Max(item.ReorderLevel - stockQuantity, 0),
            "Main Warehouse",
            MapStockStatus(stockQuantity, item.ReorderLevel),
            null,
            item.SupplierId.HasValue ? [item.SupplierId.Value.ToString()] : Array.Empty<string>());
    }

    private static string MapStockStatus(decimal stockQuantity, decimal reorderLevel)
    {
        if (stockQuantity <= 0)
        {
            return "out_of_stock";
        }

        if (stockQuantity <= reorderLevel)
        {
            return "low_stock";
        }

        return "in_stock";
    }
}

public sealed record InventoryDashboardResponse(
    int TotalSKUs,
    decimal TotalStockValue,
    int LowStockCount,
    int OutOfStockCount,
    int PendingRequests,
    int OpenPOs);

public sealed record InventoryPartResponse(
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

public sealed record InventoryPartsRequestItemResponse(
    string PartId,
    string PartName,
    decimal RequestedQty,
    decimal? IssuedQty,
    string Status);

public sealed record InventoryPartsRequestResponse(
    string Id,
    string TechnicianId,
    string TechnicianName,
    string SrId,
    string SrNumber,
    string Urgency,
    string Status,
    IReadOnlyCollection<InventoryPartsRequestItemResponse> Items,
    DateTime SubmittedAt,
    DateTime? ProcessedAt,
    string? Notes);
