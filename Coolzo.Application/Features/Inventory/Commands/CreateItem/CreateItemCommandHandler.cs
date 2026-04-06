using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.CreateItem;

public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly InventoryCatalogService _inventoryCatalogService;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<CreateItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateItemCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryCatalogService inventoryCatalogService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateItemCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryCatalogService = inventoryCatalogService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<ItemResponse> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var itemCode = request.ItemCode.Trim();

        if (await _inventoryRepository.ItemCodeExistsAsync(itemCode, null, cancellationToken))
        {
            throw new AppException(ErrorCodes.ItemAlreadyExists, "An item already exists with the same code.", 409);
        }

        var itemCategory = await _inventoryCatalogService.GetOrCreateItemCategoryAsync(
            request.CategoryCode,
            request.CategoryName,
            cancellationToken);
        var unitOfMeasure = await _inventoryCatalogService.GetOrCreateUnitOfMeasureAsync(
            request.UnitOfMeasureCode,
            request.UnitOfMeasureName,
            cancellationToken);
        var supplier = await _inventoryCatalogService.GetOrCreateSupplierAsync(
            request.SupplierCode,
            request.SupplierName,
            cancellationToken);

        var item = new Item
        {
            ItemCategory = itemCategory,
            UnitOfMeasure = unitOfMeasure,
            Supplier = supplier,
            ItemCode = itemCode,
            ItemName = request.ItemName.Trim(),
            ItemDescription = request.ItemDescription?.Trim() ?? string.Empty,
            TaxPercentage = request.TaxPercentage,
            WarrantyDays = request.WarrantyDays,
            ReorderLevel = request.ReorderLevel,
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _inventoryCatalogService.EnsureCurrentRateAsync(item, request.PurchasePrice, request.SellingPrice, cancellationToken);
        await AddAuditLogAsync("CreateItem", itemCode, item.ItemName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item {ItemCode} created by {UserName}.", itemCode, _currentUserContext.UserName);

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
