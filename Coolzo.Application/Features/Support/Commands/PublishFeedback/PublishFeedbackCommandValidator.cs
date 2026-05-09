using FluentValidation;

namespace Coolzo.Application.Features.Support.Commands.PublishFeedback;

public sealed class PublishFeedbackCommandValidator : AbstractValidator<PublishFeedbackCommand>
{
    public PublishFeedbackCommandValidator()
    {
        RuleFor(request => request.CustomerReviewId).GreaterThan(0);
    }
}
