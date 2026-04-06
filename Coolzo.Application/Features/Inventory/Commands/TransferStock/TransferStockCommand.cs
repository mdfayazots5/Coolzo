using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.TransferStock;

public sealed record TransferStockCommand(
    long SourceWarehouseId,
    long DestinationWarehouseId,
    long ItemId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Remarks) : IRequest<IReadOnlyCollection<StockTransactionResponse>>;
