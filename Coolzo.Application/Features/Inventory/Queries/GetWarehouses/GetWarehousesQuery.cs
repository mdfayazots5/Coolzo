using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouses;

public sealed record GetWarehousesQuery(
    string? SearchTerm,
    bool? IsActive) : IRequest<IReadOnlyCollection<WarehouseResponse>>;
