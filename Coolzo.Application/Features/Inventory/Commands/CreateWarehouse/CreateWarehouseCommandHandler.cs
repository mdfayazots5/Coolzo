using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.CreateWarehouse;

public sealed class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAppLogger<CreateWarehouseCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(
        IInventoryRepository inventoryRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateWarehouseCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<WarehouseResponse> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouseCode = request.WarehouseCode.Trim();

        if (await _inventoryRepository.WarehouseCodeExistsAsync(warehouseCode, cancellationToken))
        {
            throw new AppException(ErrorCodes.WarehouseAlreadyExists, "A warehouse already exists with the same code.", 409);
        }

        var warehouse = new Warehouse
        {
            WarehouseCode = warehouseCode,
            WarehouseName = request.WarehouseName.Trim(),
            ContactPerson = request.ContactPerson?.Trim() ?? string.Empty,
            MobileNumber = request.MobileNumber?.Trim() ?? string.Empty,
            EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
            AddressLine1 = request.AddressLine1?.Trim() ?? string.Empty,
            AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty,
            Landmark = request.Landmark?.Trim() ?? string.Empty,
            CityName = request.CityName?.Trim() ?? string.Empty,
            Pincode = request.Pincode?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        await _inventoryRepository.AddWarehouseAsync(warehouse, cancellationToken);
        await AddAuditLogAsync("CreateWarehouse", warehouseCode, warehouse.WarehouseName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseCode} created by {UserName}.", warehouseCode, _currentUserContext.UserName);

        return InventoryResponseMapper.ToWarehouse(warehouse);
    }

    private Task AddAuditLogAsync(string actionName, string entityId, string newValues, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = actionName,
                EntityName = "Warehouse",
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
