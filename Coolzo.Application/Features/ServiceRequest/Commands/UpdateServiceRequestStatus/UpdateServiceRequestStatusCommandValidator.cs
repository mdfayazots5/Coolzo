using FluentValidation;

namespace Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestStatus;

public sealed class UpdateServiceRequestStatusCommandValidator : AbstractValidator<UpdateServiceRequestStatusCommand>
{
    public UpdateServiceRequestStatusCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.Status)
            .NotEmpty()
            .MaximumLength(32);
        RuleFor(request => request.Remarks).MaximumLength(256);
    }
}
