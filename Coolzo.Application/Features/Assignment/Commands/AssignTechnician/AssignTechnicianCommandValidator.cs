using FluentValidation;

namespace Coolzo.Application.Features.Assignment.Commands.AssignTechnician;

public sealed class AssignTechnicianCommandValidator : AbstractValidator<AssignTechnicianCommand>
{
    public AssignTechnicianCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.TechnicianId!.Value)
            .GreaterThan(0)
            .When(request => request.TechnicianId.HasValue);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
