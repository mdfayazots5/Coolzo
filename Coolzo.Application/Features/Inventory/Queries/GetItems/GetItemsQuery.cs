using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Queries.GetItems;

public sealed record GetItemsQuery(
    string? SearchTerm,
    bool? IsActive,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<ItemResponse>>;
