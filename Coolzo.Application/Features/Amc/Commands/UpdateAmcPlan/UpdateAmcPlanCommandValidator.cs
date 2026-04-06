using FluentValidation;

namespace Coolzo.Application.Features.Amc.Commands.UpdateAmcPlan;

public sealed class UpdateAmcPlanCommandValidator : AbstractValidator<UpdateAmcPlanCommand>
{
    public UpdateAmcPlanCommandValidator()
    {
        RuleFor(request => request.AmcPlanId).GreaterThan(0);
        RuleFor(request => request.PlanName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.PlanDescription).MaximumLength(512);
        RuleFor(request => request.DurationInMonths).InclusiveBetween(1, 60);
        RuleFor(request => request.VisitCount).InclusiveBetween(1, 24);
        RuleFor(request => request.PriceAmount).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.TermsAndConditions).MaximumLength(512);
    }
}
