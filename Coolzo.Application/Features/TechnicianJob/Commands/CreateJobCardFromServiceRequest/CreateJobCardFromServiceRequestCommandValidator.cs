using FluentValidation;

namespace Coolzo.Application.Features.TechnicianJob.Commands.CreateJobCardFromServiceRequest;

public sealed class CreateJobCardFromServiceRequestCommandValidator : AbstractValidator<CreateJobCardFromServiceRequestCommand>
{
    public CreateJobCardFromServiceRequestCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
