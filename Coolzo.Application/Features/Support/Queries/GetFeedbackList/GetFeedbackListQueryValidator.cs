using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackList;

public sealed class GetFeedbackListQueryValidator : AbstractValidator<GetFeedbackListQuery>
{
    public GetFeedbackListQueryValidator()
    {
        RuleFor(request => request.ServiceId)
            .GreaterThan(0)
            .When(request => request.ServiceId.HasValue);
    }
}
