using FluentValidation;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobList;

public sealed class GetTechnicianJobListQueryValidator : AbstractValidator<GetTechnicianJobListQuery>
{
    public GetTechnicianJobListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}
