using FluentValidation;

namespace Coolzo.Application.Features.Billing.Commands.GenerateInvoiceFromQuotation;

public sealed class GenerateInvoiceFromQuotationCommandValidator : AbstractValidator<GenerateInvoiceFromQuotationCommand>
{
    public GenerateInvoiceFromQuotationCommandValidator()
    {
        RuleFor(request => request.QuotationId).GreaterThan(0);
        RuleFor(request => request.IdempotencyKey).MaximumLength(128);
    }
}
