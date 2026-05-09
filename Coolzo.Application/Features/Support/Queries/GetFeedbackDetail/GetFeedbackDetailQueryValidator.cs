using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackDetail;

public sealed class GetFeedbackDetailQueryValidator : AbstractValidator<GetFeedbackDetailQuery>
{
    public GetFeedbackDetailQueryValidator()
    {
        RuleFor(request => request.CustomerReviewId).GreaterThan(0);
    }
}
