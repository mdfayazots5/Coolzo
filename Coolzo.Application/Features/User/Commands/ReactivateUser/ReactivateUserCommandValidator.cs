using FluentValidation;

namespace Coolzo.Application.Features.User.Commands.ReactivateUser;

public sealed class ReactivateUserCommandValidator : AbstractValidator<ReactivateUserCommand>
{
    public ReactivateUserCommandValidator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0);
    }
}
