using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.RespondFeedback;

public sealed class RespondFeedbackCommandValidator : AbstractValidator<RespondFeedbackCommand>
{
    public RespondFeedbackCommandValidator()
    {
        RuleFor(request => request.CustomerReviewId).GreaterThan(0);
        RuleFor(request => request.Response).MaximumLength(512);
    }
}
