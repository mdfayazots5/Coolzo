using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetItemById;

public sealed class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemResponse>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetItemByIdQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<ItemResponse> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _inventoryRepository.GetItemByIdAsync(request.ItemId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested item could not be found.", 404);

        return InventoryResponseMapper.ToItem(item);
    }
}
