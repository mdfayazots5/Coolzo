using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.TransferStock;

public sealed class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, IReadOnlyCollection<StockTransactionResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<TransferStockCommandHandler> _logger;
    private readonly InventoryStockService _inventoryStockService;
    private readonly IUnitOfWork _unitOfWork;

    public TransferStockCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryStockService inventoryStockService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<TransferStockCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryStockService = inventoryStockService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<StockTransactionResponse>> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        var sourceWarehouse = await _inventoryRepository.GetWarehouseByIdForUpdateAsync(request.SourceWarehouseId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The source warehouse could not be found.", 404);
        var destinationWarehouse = await _inventoryRepository.GetWarehouseByIdForUpdateAsync(request.DestinationWarehouseId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The destination warehouse could not be found.", 404);
        var item = await _inventoryRepository.GetItemByIdForUpdateAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested item could not be found.", 404);

        var transactions = await _inventoryStockService.TransferWarehouseStockAsync(
            sourceWarehouse,
            destinationWarehouse,
            item,
            request.Quantity,
            request.UnitCost,
            request.ReferenceNumber,
            request.Remarks,
            cancellationToken);

        await AddAuditLogAsync("TransferStock", transactions.First().TransactionGroupCode, item.ItemCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock transferred from warehouse {SourceWarehouseId} to warehouse {DestinationWarehouseId} for item {ItemId} by {UserName}.",
            sourceWarehouse.WarehouseId,
            destinationWarehouse.WarehouseId,
            item.ItemId,
            _currentUserContext.UserName);

        return transactions.Select(InventoryResponseMapper.ToStockTransaction).ToArray();
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string newValues, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "StockTransaction",
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
