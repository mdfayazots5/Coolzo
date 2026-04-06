using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.UpdateItem;

public sealed class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly InventoryCatalogService _inventoryCatalogService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<UpdateItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateItemCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryCatalogService inventoryCatalogService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<UpdateItemCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryCatalogService = inventoryCatalogService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ItemResponse> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _inventoryRepository.GetItemByIdForUpdateAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested item could not be found.", 404);
        var itemCode = request.ItemCode.Trim();

        if (await _inventoryRepository.ItemCodeExistsAsync(itemCode, item.ItemId, cancellationToken))
        {
            throw new AppException(ErrorCodes.ItemAlreadyExists, "An item already exists with the same code.", 409);
        }

        item.ItemCategory = await _inventoryCatalogService.GetOrCreateItemCategoryAsync(
            request.CategoryCode,
            request.CategoryName,
            cancellationToken);
        item.UnitOfMeasure = await _inventoryCatalogService.GetOrCreateUnitOfMeasureAsync(
            request.UnitOfMeasureCode,
            request.UnitOfMeasureName,
            cancellationToken);
        item.Supplier = await _inventoryCatalogService.GetOrCreateSupplierAsync(
            request.SupplierCode,
            request.SupplierName,
            cancellationToken);
        item.ItemCode = itemCode;
        item.ItemName = request.ItemName.Trim();
        item.ItemDescription = request.ItemDescription?.Trim() ?? string.Empty;
        item.TaxPercentage = request.TaxPercentage;
        item.WarrantyDays = request.WarrantyDays;
        item.ReorderLevel = request.ReorderLevel;
        item.IsActive = request.IsActive;
        item.LastUpdated = _currentDateTime.UtcNow;
        item.UpdatedBy = _currentUserContext.UserName;

        await _inventoryCatalogService.EnsureCurrentRateAsync(item, request.PurchasePrice, request.SellingPrice, cancellationToken);
        await AddAuditLogAsync("UpdateItem", item.ItemCode, item.ItemName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item {ItemId} updated by {UserName}.", item.ItemId, _currentUserContext.UserName);

        return InventoryResponseMapper.ToItem(item);
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string newValues, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "Item",
                EntityId = entityId,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = newValues,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
