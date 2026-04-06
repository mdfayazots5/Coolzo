using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetStockTransactions;

public sealed class GetStockTransactionsQueryHandler : IRequestHandler<GetStockTransactionsQuery, PagedResult<StockTransactionResponse>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetStockTransactionsQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<PagedResult<StockTransactionResponse>> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactionType = StockTransactionTypeResolver.ParseOrNull(request.TransactionType);

        if (!string.IsNullOrWhiteSpace(request.TransactionType) && !transactionType.HasValue)
        {
            throw new AppException(ErrorCodes.InvalidStockTransactionType, "The supplied stock transaction type is invalid.", 400);
        }

        var transactions = await _inventoryRepository.SearchStockTransactionsAsync(
            transactionType,
            request.ItemId,
            request.WarehouseId,
            request.TechnicianId,
            request.JobCardId,
            request.FromDateUtc,
            request.ToDateUtc,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var totalCount = await _inventoryRepository.CountStockTransactionsAsync(
            transactionType,
            request.ItemId,
            request.WarehouseId,
            request.TechnicianId,
            request.JobCardId,
            request.FromDateUtc,
            request.ToDateUtc,
            cancellationToken);

        return new PagedResult<StockTransactionResponse>(
            transactions.Select(InventoryResponseMapper.ToStockTransaction).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
