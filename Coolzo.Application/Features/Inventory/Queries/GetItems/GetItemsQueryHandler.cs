using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetItems;

public sealed class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, PagedResult<ItemResponse>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetItemsQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<PagedResult<ItemResponse>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _inventoryRepository.SearchItemsAsync(
            request.SearchTerm,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var totalCount = await _inventoryRepository.CountItemsAsync(request.SearchTerm, request.IsActive, cancellationToken);

        return new PagedResult<ItemResponse>(
            items.Select(InventoryResponseMapper.ToItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
