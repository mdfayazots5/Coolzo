using FluentValidation;

namespace Coolzo.Application.Features.User.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(128);

        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.RoleIds)
            .NotEmpty();
    }
}
