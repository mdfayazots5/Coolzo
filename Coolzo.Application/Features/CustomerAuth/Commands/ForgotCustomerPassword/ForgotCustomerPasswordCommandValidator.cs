using FluentValidation;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ForgotCustomerPassword;

public sealed class ForgotCustomerPasswordCommandValidator : AbstractValidator<ForgotCustomerPasswordCommand>
{
    public ForgotCustomerPasswordCommandValidator()
    {
        RuleFor(request => request.LoginId)
            .NotEmpty()
            .MaximumLength(128);
    }
}
