using Coolzo.Contracts.Responses.ServiceHistory;
using MediatR;

namespace Coolzo.Application.Features.ServiceHistory.Queries.GetServiceHistory;

public sealed record GetServiceHistoryQuery(long CustomerId) : IRequest<IReadOnlyCollection<ServiceHistoryItemResponse>>;
