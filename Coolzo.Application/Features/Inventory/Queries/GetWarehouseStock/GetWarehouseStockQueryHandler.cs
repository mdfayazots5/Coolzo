using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;

public sealed class GetWarehouseStockQueryHandler : IRequestHandler<GetWarehouseStockQuery, WarehouseStockResponse>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetWarehouseStockQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<WarehouseStockResponse> Handle(GetWarehouseStockQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _inventoryRepository.GetWarehouseByIdAsync(request.WarehouseId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested warehouse could not be found.", 404);
        var stockItems = await _inventoryRepository.GetWarehouseStockByWarehouseIdAsync(request.WarehouseId, cancellationToken);

        return InventoryResponseMapper.ToWarehouseStock(warehouse, stockItems);
    }
}
