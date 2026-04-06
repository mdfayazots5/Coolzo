using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.ChecklistResponse.Queries.GetJobChecklist;

public sealed record GetJobChecklistQuery(
    long ServiceRequestId) : IRequest<IReadOnlyCollection<JobChecklistItemResponse>>;
