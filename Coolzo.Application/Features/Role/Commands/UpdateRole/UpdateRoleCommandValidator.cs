using FluentValidation;

namespace Coolzo.Application.Features.Role.Commands.UpdateRole;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(request => request.RoleId)
            .GreaterThan(0);

        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.Description)
            .MaximumLength(256);

        RuleFor(request => request.PermissionIds)
            .NotEmpty();
    }
}
