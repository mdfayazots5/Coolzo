using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.FlagFeedback;

public sealed class FlagFeedbackCommandValidator : AbstractValidator<FlagFeedbackCommand>
{
    public FlagFeedbackCommandValidator()
    {
        RuleFor(request => request.CustomerReviewId).GreaterThan(0);
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(56);
    }
}
