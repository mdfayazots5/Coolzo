using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.ConsumeJobParts;

public sealed record ConsumeJobPartsCommand(
    long JobCardId,
    IReadOnlyCollection<ConsumeJobPartLineCommand> Items) : IRequest<JobPartConsumptionSummaryResponse>;

public sealed record ConsumeJobPartLineCommand(
    long ItemId,
    decimal QuantityUsed,
    string? ConsumptionRemarks);
