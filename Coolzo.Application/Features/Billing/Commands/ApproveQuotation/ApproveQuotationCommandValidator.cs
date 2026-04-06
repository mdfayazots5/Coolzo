using FluentValidation;

namespace Coolzo.Application.Features.Billing.Commands.ApproveQuotation;

public sealed class ApproveQuotationCommandValidator : AbstractValidator<ApproveQuotationCommand>
{
    public ApproveQuotationCommandValidator()
    {
        RuleFor(request => request.QuotationId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}
