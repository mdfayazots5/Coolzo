using FluentValidation;

namespace Coolzo.Application.Features.CustomerAuth.Commands.ResetCustomerPasswordWithOtp;

public sealed class ResetCustomerPasswordWithOtpCommandValidator : AbstractValidator<ResetCustomerPasswordWithOtpCommand>
{
    public ResetCustomerPasswordWithOtpCommandValidator()
    {
        RuleFor(request => request.LoginId).NotEmpty().MaximumLength(64);
        RuleFor(request => request.Otp).NotEmpty().Length(4, 10);
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
