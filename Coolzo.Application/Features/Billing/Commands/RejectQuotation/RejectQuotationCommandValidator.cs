using FluentValidation;

namespace Coolzo.Application.Features.Billing.Commands.RejectQuotation;

public sealed class RejectQuotationCommandValidator : AbstractValidator<RejectQuotationCommand>
{
    public RejectQuotationCommandValidator()
    {
        RuleFor(request => request.QuotationId).GreaterThan(0);
        RuleFor(request => request.Remarks).NotEmpty().MaximumLength(512);
    }
}
