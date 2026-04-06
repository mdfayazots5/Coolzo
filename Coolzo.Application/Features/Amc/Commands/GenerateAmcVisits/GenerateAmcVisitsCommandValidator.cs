using FluentValidation;

namespace Coolzo.Application.Features.Amc.Commands.GenerateAmcVisits;

public sealed class GenerateAmcVisitsCommandValidator : AbstractValidator<GenerateAmcVisitsCommand>
{
    public GenerateAmcVisitsCommandValidator()
    {
        RuleFor(request => request.CustomerAmcId).GreaterThan(0);
    }
}
