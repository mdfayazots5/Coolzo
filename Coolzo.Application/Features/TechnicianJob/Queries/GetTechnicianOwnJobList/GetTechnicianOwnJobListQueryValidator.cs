using FluentValidation;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianOwnJobList;

public sealed class GetTechnicianOwnJobListQueryValidator : AbstractValidator<GetTechnicianOwnJobListQuery>
{
    public GetTechnicianOwnJobListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}
