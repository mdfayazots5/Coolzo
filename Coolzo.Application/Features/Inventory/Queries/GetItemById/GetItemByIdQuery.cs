using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetItemById;

public sealed record GetItemByIdQuery(long ItemId) : IRequest<ItemResponse>;
