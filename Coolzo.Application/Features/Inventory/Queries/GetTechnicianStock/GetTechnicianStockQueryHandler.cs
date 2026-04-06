using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetTechnicianStock;

public sealed class GetTechnicianStockQueryHandler : IRequestHandler<GetTechnicianStockQuery, TechnicianStockResponse>
{
    private readonly InventoryAccessService _inventoryAccessService;
    private readonly IInventoryRepository _inventoryRepository;

    public GetTechnicianStockQueryHandler(
        IInventoryRepository inventoryRepository,
        InventoryAccessService inventoryAccessService)
    {
        _inventoryRepository = inventoryRepository;
        _inventoryAccessService = inventoryAccessService;
    }

    public async Task<TechnicianStockResponse> Handle(GetTechnicianStockQuery request, CancellationToken cancellationToken)
    {
        var technician = await _inventoryRepository.GetTechnicianByIdAsync(request.TechnicianId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested technician could not be found.", 404);

        await _inventoryAccessService.EnsureTechnicianStockReadAccessAsync(request.TechnicianId, cancellationToken);

        var stockItems = await _inventoryRepository.GetTechnicianStockByTechnicianIdAsync(request.TechnicianId, cancellationToken);

        return InventoryResponseMapper.ToTechnicianStock(technician, stockItems);
    }
}
