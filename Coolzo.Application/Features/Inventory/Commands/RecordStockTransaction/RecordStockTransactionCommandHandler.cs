using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.RecordStockTransaction;

public sealed class RecordStockTransactionCommandHandler : IRequestHandler<RecordStockTransactionCommand, StockTransactionResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<RecordStockTransactionCommandHandler> _logger;
    private readonly InventoryStockService _inventoryStockService;
    private readonly IUnitOfWork _unitOfWork;

    public RecordStockTransactionCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryStockService inventoryStockService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<RecordStockTransactionCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryStockService = inventoryStockService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<StockTransactionResponse> Handle(RecordStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _inventoryRepository.GetWarehouseByIdForUpdateAsync(request.WarehouseId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested warehouse could not be found.", 404);
        var item = await _inventoryRepository.GetItemByIdForUpdateAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested item could not be found.", 404);
        var transactionType = StockTransactionTypeResolver.ParseOrThrow(request.TransactionType);

        var stockTransaction = await _inventoryStockService.RecordWarehouseTransactionAsync(
            warehouse,
            item,
            transactionType,
            request.Quantity,
            request.UnitCost,
            request.SupplierId,
            request.ReferenceNumber,
            request.Remarks,
            cancellationToken);

        await AddAuditLogAsync("RecordStockTransaction", stockTransaction.TransactionGroupCode, stockTransaction.TransactionType.ToString(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock transaction {TransactionType} recorded for warehouse {WarehouseId} item {ItemId} by {UserName}.",
            stockTransaction.TransactionType,
            warehouse.WarehouseId,
            item.ItemId,
            _currentUserContext.UserName);

        return InventoryResponseMapper.ToStockTransaction(stockTransaction);
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
