using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.AssignStockToTechnician;

public sealed record AssignStockToTechnicianCommand(
    long TechnicianId,
    long SourceWarehouseId,
    long ItemId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Remarks) : IRequest<IReadOnlyCollection<StockTransactionResponse>>;
