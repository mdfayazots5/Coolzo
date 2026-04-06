using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Queries.GetJobExecutionTimeline;

public sealed record GetJobExecutionTimelineQuery(
    long ServiceRequestId) : IRequest<IReadOnlyCollection<JobExecutionTimelineItemResponse>>;
