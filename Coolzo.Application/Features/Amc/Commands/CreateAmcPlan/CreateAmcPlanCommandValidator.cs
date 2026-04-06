using FluentValidation;

namespace Coolzo.Application.Features.Amc.Commands.CreateAmcPlan;

public sealed class CreateAmcPlanCommandValidator : AbstractValidator<CreateAmcPlanCommand>
{
    public CreateAmcPlanCommandValidator()
    {
        RuleFor(request => request.PlanName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.PlanDescription).MaximumLength(512);
        RuleFor(request => request.DurationInMonths).InclusiveBetween(1, 60);
        RuleFor(request => request.VisitCount).InclusiveBetween(1, 24);
        RuleFor(request => request.PriceAmount).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.TermsAndConditions).MaximumLength(512);
    }
}
