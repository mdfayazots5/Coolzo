using FluentValidation;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.ResetCustomerPassword;

public sealed class ResetCustomerPasswordCommandValidator : AbstractValidator<ResetCustomerPasswordCommand>
{
    public ResetCustomerPasswordCommandValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.Reason).MaximumLength(256).When(request => request.Reason is not null);
    }
}
