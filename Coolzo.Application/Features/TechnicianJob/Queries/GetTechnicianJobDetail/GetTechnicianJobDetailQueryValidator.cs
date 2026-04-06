using FluentValidation;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobDetail;

public sealed class GetTechnicianJobDetailQueryValidator : AbstractValidator<GetTechnicianJobDetailQuery>
{
    public GetTechnicianJobDetailQueryValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
    }
}
