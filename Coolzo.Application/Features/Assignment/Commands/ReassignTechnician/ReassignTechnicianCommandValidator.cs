using FluentValidation;

namespace Coolzo.Application.Features.Assignment.Commands.ReassignTechnician;

public sealed class ReassignTechnicianCommandValidator : AbstractValidator<ReassignTechnicianCommand>
{
    public ReassignTechnicianCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.TechnicianId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
