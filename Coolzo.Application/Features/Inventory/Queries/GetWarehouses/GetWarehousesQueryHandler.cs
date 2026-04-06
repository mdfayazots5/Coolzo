using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouses;

public sealed class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IReadOnlyCollection<WarehouseResponse>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetWarehousesQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IReadOnlyCollection<WarehouseResponse>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _inventoryRepository.SearchWarehousesAsync(request.SearchTerm, request.IsActive, cancellationToken);

        return warehouses
            .Select(InventoryResponseMapper.ToWarehouse)
            .ToArray();
    }
}
