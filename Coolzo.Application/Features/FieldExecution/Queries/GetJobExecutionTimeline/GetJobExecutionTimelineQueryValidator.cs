using FluentValidation;

namespace Coolzo.Application.Features.FieldExecution.Queries.GetJobExecutionTimeline;

public sealed class GetJobExecutionTimelineQueryValidator : AbstractValidator<GetJobExecutionTimelineQuery>
{
    public GetJobExecutionTimelineQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
