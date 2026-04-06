using Coolzo.Contracts.Common;
using MediatR;

namespace Coolzo.Application.Features.Lookup.Queries.GetLookupItems;

public sealed record GetLookupItemsQuery(string LookupType) : IRequest<IReadOnlyCollection<LookupItemResponse>>;
