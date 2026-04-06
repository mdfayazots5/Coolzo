using FluentValidation;

namespace Coolzo.Application.Features.CustomerAuth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
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

        RuleFor(request => request.Password)
            .MaximumLength(512)
            .When(request => request.Password is not null);
    }
}
