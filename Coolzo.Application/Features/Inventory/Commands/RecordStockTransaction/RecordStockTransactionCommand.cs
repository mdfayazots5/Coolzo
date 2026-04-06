using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.RecordStockTransaction;

public sealed record RecordStockTransactionCommand(
    long WarehouseId,
    long ItemId,
    string TransactionType,
    decimal Quantity,
    decimal UnitCost,
    long? SupplierId,
    string? ReferenceNumber,
    string? Remarks) : IRequest<StockTransactionResponse>;
