using FluentValidation;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianList;

public sealed class GetTechnicianListQueryValidator : AbstractValidator<GetTechnicianListQuery>
{
    public GetTechnicianListQueryValidator()
    {
        RuleFor(request => request.SearchTerm).MaximumLength(128);
    }
}
