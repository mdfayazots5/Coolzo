using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetStockTransactions;

public sealed record GetStockTransactionsQuery(
    string? TransactionType,
    long? ItemId,
    long? WarehouseId,
    long? TechnicianId,
    long? JobCardId,
    DateTime? FromDateUtc,
    DateTime? ToDateUtc,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<StockTransactionResponse>>;
