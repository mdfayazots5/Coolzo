using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.AssignStockToTechnician;

public sealed class AssignStockToTechnicianCommandHandler : IRequestHandler<AssignStockToTechnicianCommand, IReadOnlyCollection<StockTransactionResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<AssignStockToTechnicianCommandHandler> _logger;
    private readonly InventoryStockService _inventoryStockService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignStockToTechnicianCommandHandler(
        IInventoryRepository inventoryRepository,
        InventoryStockService inventoryStockService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<AssignStockToTechnicianCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryStockService = inventoryStockService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<StockTransactionResponse>> Handle(
        AssignStockToTechnicianCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _inventoryRepository.GetWarehouseByIdForUpdateAsync(request.SourceWarehouseId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The source warehouse could not be found.", 404);
        var technician = await _inventoryRepository.GetTechnicianByIdForUpdateAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested technician could not be found.", 404);
        var item = await _inventoryRepository.GetItemByIdForUpdateAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested item could not be found.", 404);

        if (!technician.IsActive)
        {
            throw new AppException(ErrorCodes.TechnicianInactive, "The requested technician is inactive.", 409);
        }

        var transactions = await _inventoryStockService.AssignStockToTechnicianAsync(
            warehouse,
            technician,
            item,
            request.Quantity,
            request.UnitCost,
            request.ReferenceNumber,
            request.Remarks,
            cancellationToken);

        await AddAuditLogAsync("AssignStockToTechnician", transactions.First().TransactionGroupCode, technician.TechnicianCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock assigned from warehouse {WarehouseId} to technician {TechnicianId} for item {ItemId} by {UserName}.",
            warehouse.WarehouseId,
            technician.TechnicianId,
            item.ItemId,
            _currentUserContext.UserName);

        return transactions.Select(InventoryResponseMapper.ToStockTransaction).ToArray();
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string newValues, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new Coolzo.Domain.Entities.AuditLog
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
