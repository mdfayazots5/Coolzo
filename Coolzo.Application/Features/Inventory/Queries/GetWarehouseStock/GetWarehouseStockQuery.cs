using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetWarehouseStock;

public sealed record GetWarehouseStockQuery(long WarehouseId) : IRequest<WarehouseStockResponse>;
