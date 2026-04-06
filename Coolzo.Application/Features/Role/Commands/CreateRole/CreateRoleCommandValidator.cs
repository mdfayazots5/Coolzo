using FluentValidation;

namespace Coolzo.Application.Features.Role.Commands.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(request => request.RoleName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Description)
            .MaximumLength(256);

        RuleFor(request => request.PermissionIds)
            .NotEmpty();
    }
}
