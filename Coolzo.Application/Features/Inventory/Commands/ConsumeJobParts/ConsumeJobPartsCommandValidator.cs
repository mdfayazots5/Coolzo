using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Commands.ConsumeJobParts;

public sealed class ConsumeJobPartsCommandValidator : AbstractValidator<ConsumeJobPartsCommand>
{
    public ConsumeJobPartsCommandValidator()
    {
        RuleFor(request => request.JobCardId).GreaterThan(0);
        RuleFor(request => request.Items).NotEmpty();

        RuleForEach(request => request.Items)
            .SetValidator(new ConsumeJobPartLineCommandValidator());
    }
}

internal sealed class ConsumeJobPartLineCommandValidator : AbstractValidator<ConsumeJobPartLineCommand>
{
    public ConsumeJobPartLineCommandValidator()
    {
        RuleFor(request => request.ItemId).GreaterThan(0);
        RuleFor(request => request.QuantityUsed).GreaterThan(0.00m);
        RuleFor(request => request.ConsumptionRemarks).MaximumLength(512);
    }
}
