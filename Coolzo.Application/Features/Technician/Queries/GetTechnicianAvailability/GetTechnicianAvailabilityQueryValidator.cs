using FluentValidation;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianAvailability;

public sealed class GetTechnicianAvailabilityQueryValidator : AbstractValidator<GetTechnicianAvailabilityQuery>
{
    public GetTechnicianAvailabilityQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
