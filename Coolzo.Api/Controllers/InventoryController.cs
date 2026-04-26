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

    [HttpPatch("parts-requests/{id:long}/approve")]
    [Authorize(Policy = PermissionNames.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<InventoryPartsRequestResponse>), StatusCodes.Status200OK)]
    public Task<ActionResult<ApiResponse<InventoryPartsRequestResponse>>> ApprovePartsRequestAsync(
        [FromRoute] long id,
        [FromBody] InventoryPartsRequestProcessRequest request,
        CancellationToken cancellationToken)
    {
        return ProcessPartsRequestAsync(id, PartsRequestStatus.Approved, request, cancellationToken);
    }

    [HttpPatch("parts-requests/{id:long}/partial")]
    [Authorize(Policy = PermissionNames.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<InventoryPartsRequestResponse>), StatusCodes.Status200OK)]
    public Task<ActionResult<ApiResponse<InventoryPartsRequestResponse>>> PartiallyApprovePartsRequestAsync(
        [FromRoute] long id,
        [FromBody] InventoryPartsRequestProcessRequest request,
        CancellationToken cancellationToken)
    {
        return ProcessPartsRequestAsync(id, PartsRequestStatus.PartiallyApproved, request, cancellationToken);
    }

    [HttpPatch("parts-requests/{id:long}/reject")]
    [Authorize(Policy = PermissionNames.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<InventoryPartsRequestResponse>), StatusCodes.Status200OK)]
    public Task<ActionResult<ApiResponse<InventoryPartsRequestResponse>>> RejectPartsRequestAsync(
        [FromRoute] long id,
        [FromBody] InventoryPartsRequestProcessRequest request,
        CancellationToken cancellationToken)
    {
        return ProcessPartsRequestAsync(id, PartsRequestStatus.Rejected, request, cancellationToken);
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

    [HttpGet("stock-movements")]
    [Authorize(Policy = PermissionNames.StockRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<InventoryStockMovementResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventoryStockMovementResponse>>>> GetStockMovementsAsync(
        [FromQuery] long? partId,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.StockTransactions
            .AsNoTracking()
            .Include(item => item.Item)
            .Include(item => item.Technician)
            .Where(item => !item.IsDeleted && (!partId.HasValue || item.ItemId == partId.Value))
            .OrderByDescending(item => item.TransactionDateUtc)
            .Take(250)
            .ToArrayAsync(cancellationToken);

        var response = transactions
            .Select(item => new InventoryStockMovementResponse(
                item.StockTransactionId.ToString(),
                item.ItemId.ToString(),
                item.Item?.ItemName ?? $"Part {item.ItemId}",
                MapMovementType(item.TransactionType),
                Math.Abs(item.Quantity),
                item.BalanceAfterTransaction,
                string.IsNullOrWhiteSpace(item.ReferenceNumber) ? $"TXN-{item.StockTransactionId}" : item.ReferenceNumber,
                MapReferenceType(item.TransactionType),
                item.TransactionDateUtc,
                item.Technician?.TechnicianName ?? item.CreatedBy,
                string.IsNullOrWhiteSpace(item.Remarks) ? null : item.Remarks))
            .ToArray();

        return Success<IReadOnlyCollection<InventoryStockMovementResponse>>(response);
    }

    [HttpGet("suppliers")]
    [Authorize(Policy = PermissionNames.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<InventorySupplierResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventorySupplierResponse>>>> GetSuppliersAsync(
        CancellationToken cancellationToken)
    {
        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.IsActive)
            .OrderBy(item => item.SupplierName)
            .ToArrayAsync(cancellationToken);

        return Success<IReadOnlyCollection<InventorySupplierResponse>>(suppliers.Select(MapSupplier).ToArray());
    }

    [HttpPost("suppliers")]
    [Authorize(Policy = PermissionNames.ItemCreate)]
    [ProducesResponseType(typeof(ApiResponse<InventorySupplierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventorySupplierResponse>>> CreateSupplierAsync(
        [FromBody] InventorySupplierCreateRequest request,
        CancellationToken cancellationToken)
    {
        var nextCodeSeed = await _dbContext.Suppliers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var supplier = new Supplier
        {
            SupplierCode = $"SUP-{nextCodeSeed + 1:0000}",
            SupplierName = request.Name.Trim(),
            ContactPerson = request.ContactPerson?.Trim() ?? string.Empty,
            MobileNumber = request.Phone?.Trim() ?? string.Empty,
            EmailAddress = request.Email?.Trim() ?? string.Empty,
            AddressLine = string.Empty,
            IsActive = true,
            CreatedBy = User.Identity?.Name ?? "System",
            DateCreated = DateTime.UtcNow,
            IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
        };

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success(MapSupplier(supplier), "Supplier created successfully.");
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

    private async Task<ActionResult<ApiResponse<InventoryPartsRequestResponse>>> ProcessPartsRequestAsync(
        long id,
        PartsRequestStatus targetStatus,
        InventoryPartsRequestProcessRequest request,
        CancellationToken cancellationToken)
    {
        var partsRequest = await _dbContext.PartsRequests
            .Include(entity => entity.ServiceRequest)
            .Include(entity => entity.Technician)
            .Include(entity => entity.Items.Where(item => !item.IsDeleted))
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.PartsRequestId == id, cancellationToken);

        if (partsRequest is null)
        {
            return NotFound();
        }

        var issuedQuantities = request.Items
            .Where(item => long.TryParse(item.PartId, out _))
            .ToDictionary(item => long.Parse(item.PartId), item => Math.Max(item.IssuedQty ?? 0m, 0m));
        var partIdLookup = await BuildPartIdLookupAsync([partsRequest], cancellationToken);

        Warehouse? primaryWarehouse = null;
        if (targetStatus is PartsRequestStatus.Approved or PartsRequestStatus.PartiallyApproved)
        {
            primaryWarehouse = await _dbContext.Warehouses
                .FirstOrDefaultAsync(item => !item.IsDeleted && item.IsActive, cancellationToken);
        }

        foreach (var item in partsRequest.Items)
        {
            var resolvedPartId = item.ItemId
                ?? (partIdLookup.TryGetValue(item.PartCode, out var fallbackId) ? fallbackId : 0L);
            var issuedQty = resolvedPartId > 0 && issuedQuantities.TryGetValue(resolvedPartId, out var requestedIssueQty)
                ? requestedIssueQty
                : 0m;

            if (targetStatus == PartsRequestStatus.Rejected)
            {
                issuedQty = 0m;
            }

            item.QuantityApproved = Math.Min(item.QuantityRequested, issuedQty);
            item.CurrentStatus = targetStatus;

            if (item.QuantityApproved <= 0 || primaryWarehouse is null || !item.ItemId.HasValue)
            {
                continue;
            }

            var warehouseStock = await _dbContext.WarehouseStocks
                .FirstOrDefaultAsync(
                    stock => !stock.IsDeleted && stock.WarehouseId == primaryWarehouse.WarehouseId && stock.ItemId == item.ItemId.Value,
                    cancellationToken);

            if (warehouseStock is null)
            {
                continue;
            }

            warehouseStock.QuantityOnHand = Math.Max(warehouseStock.QuantityOnHand - item.QuantityApproved, 0m);

            _dbContext.StockTransactions.Add(new StockTransaction
            {
                ItemId = item.ItemId.Value,
                WarehouseId = primaryWarehouse.WarehouseId,
                TechnicianId = partsRequest.TechnicianId,
                JobCardId = partsRequest.JobCardId,
                SupplierId = null,
                TransactionType = StockTransactionType.JobConsumption,
                Quantity = item.QuantityApproved,
                UnitCost = 0m,
                Amount = 0m,
                ReferenceNumber = partsRequest.ServiceRequest?.ServiceRequestNumber ?? $"PR-{partsRequest.PartsRequestId}",
                TransactionGroupCode = $"PARTSREQ-{partsRequest.PartsRequestId}",
                TransactionDateUtc = DateTime.UtcNow,
                Remarks = $"Parts request {targetStatus}",
                BalanceAfterTransaction = warehouseStock.QuantityOnHand,
                CreatedBy = User.Identity?.Name ?? "System",
                DateCreated = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            });
        }

        partsRequest.CurrentStatus = targetStatus;
        partsRequest.ProcessedAtUtc = DateTime.UtcNow;
        partsRequest.LastUpdated = DateTime.UtcNow;
        partsRequest.UpdatedBy = User.Identity?.Name ?? "System";

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success(MapPartsRequest(partsRequest, partIdLookup), "Parts request updated successfully.");
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

    private static string MapMovementType(StockTransactionType transactionType)
    {
        return transactionType switch
        {
            StockTransactionType.PurchaseIn or StockTransactionType.TransferIn or StockTransactionType.ReturnIn => "IN",
            StockTransactionType.JobConsumption or StockTransactionType.TransferOut or StockTransactionType.ReturnOut => "OUT",
            StockTransactionType.AdjustmentIn or StockTransactionType.AdjustmentOut => "ADJ",
            _ => "RET",
        };
    }

    private static string MapReferenceType(StockTransactionType transactionType)
    {
        return transactionType switch
        {
            StockTransactionType.PurchaseIn => "po",
            StockTransactionType.JobConsumption => "job",
            StockTransactionType.AdjustmentIn or StockTransactionType.AdjustmentOut => "manual",
            _ => "return",
        };
    }

    private static InventorySupplierResponse MapSupplier(Supplier supplier)
    {
        return new InventorySupplierResponse(
            supplier.SupplierId.ToString(),
            supplier.SupplierName,
            supplier.ContactPerson,
            supplier.MobileNumber,
            supplier.EmailAddress,
            0,
            "Standard");
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

public sealed record InventoryStockMovementResponse(
    string Id,
    string PartId,
    string PartName,
    string Type,
    decimal Quantity,
    decimal BalanceAfter,
    string ReferenceId,
    string ReferenceType,
    DateTime Timestamp,
    string Actor,
    string? Notes);

public sealed record InventorySupplierResponse(
    string Id,
    string Name,
    string ContactPerson,
    string Phone,
    string Email,
    int LeadTimeDays,
    string PaymentTerms);

public sealed record InventorySupplierCreateRequest(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    int? LeadTimeDays,
    string? PaymentTerms);

public sealed record InventoryPartsRequestProcessItemRequest(
    string PartId,
    string PartName,
    decimal RequestedQty,
    decimal? IssuedQty,
    string Status);

public sealed record InventoryPartsRequestProcessRequest(
    IReadOnlyCollection<InventoryPartsRequestProcessItemRequest> Items);
