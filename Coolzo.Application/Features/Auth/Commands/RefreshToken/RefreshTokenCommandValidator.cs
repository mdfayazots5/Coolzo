using FluentValidation;

namespace Coolzo.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(request => request.AccessToken)
            .NotEmpty();

        RuleFor(request => request.RefreshToken)
            .NotEmpty();
    }
}
