using FluentValidation;

namespace Coolzo.Application.Features.Warranty.Commands.CreateWarrantyClaim;

public sealed class CreateWarrantyClaimCommandValidator : AbstractValidator<CreateWarrantyClaimCommand>
{
    public CreateWarrantyClaimCommandValidator()
    {
        RuleFor(request => request.InvoiceId).GreaterThan(0);
        RuleFor(request => request.ClaimRemarks).MaximumLength(512);
    }
}
