using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Assignment.Queries.GetAssignmentHistory;

public sealed record GetAssignmentHistoryQuery(
    long ServiceRequestId) : IRequest<IReadOnlyCollection<AssignmentHistoryItemResponse>>;
