using FluentValidation;

namespace Coolzo.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(request => request.UserNameOrEmail)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(512);
    }
}

public sealed class LoginFieldCommandValidator : AbstractValidator<LoginFieldCommand>
{
    public LoginFieldCommandValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Pin)
            .NotEmpty()
            .MaximumLength(512);
    }
}
