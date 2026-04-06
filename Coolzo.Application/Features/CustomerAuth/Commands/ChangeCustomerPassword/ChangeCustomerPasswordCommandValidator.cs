using FluentValidation;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ChangeCustomerPassword;

public sealed class ChangeCustomerPasswordCommandValidator : AbstractValidator<ChangeCustomerPasswordCommand>
{
    public ChangeCustomerPasswordCommandValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MaximumLength(512);
    }
}
