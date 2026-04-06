using FluentValidation;

namespace Coolzo.Application.Features.User.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(request => request.UserName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(128);

        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(request => request.RoleIds)
            .NotEmpty();
    }
}
