using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetJobConsumption;

public sealed class GetJobConsumptionQueryValidator : AbstractValidator<GetJobConsumptionQuery>
{
    public GetJobConsumptionQueryValidator()
    {
        RuleFor(request => request.JobCardId).GreaterThan(0);
    }
}
