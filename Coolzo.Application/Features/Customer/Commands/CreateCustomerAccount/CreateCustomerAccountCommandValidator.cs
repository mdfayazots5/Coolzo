using FluentValidation;

namespace Coolzo.Application.Features.CustomerAccounts.Commands.CreateCustomerAccount;

public sealed class CreateCustomerAccountCommandValidator : AbstractValidator<CreateCustomerAccountCommand>
{
    public CreateCustomerAccountCommandValidator()
    {
        RuleFor(request => request.CustomerName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.MobileNumber)
            .NotEmpty()
            .Matches("^[0-9]{10,32}$");

        RuleFor(request => request.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(128);
    }
}
