using Coolzo.Contracts.Requests.Billing;
using Coolzo.Domain.Enums;
using FluentValidation;

namespace Coolzo.Application.Features.Billing.Commands.CreateQuotationFromJob;

public sealed class CreateQuotationFromJobCommandValidator : AbstractValidator<CreateQuotationFromJobCommand>
{
    public CreateQuotationFromJobCommandValidator()
    {
        RuleFor(request => request.JobCardId).GreaterThan(0);
        RuleFor(request => request.Lines).NotEmpty();
        RuleFor(request => request.DiscountAmount).GreaterThanOrEqualTo(0.00m);
        RuleFor(request => request.TaxPercentage).GreaterThanOrEqualTo(0.00m).LessThanOrEqualTo(100.00m);
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleForEach(request => request.Lines).SetValidator(new QuotationLineRequestValidator());
    }

    private sealed class QuotationLineRequestValidator : AbstractValidator<QuotationLineRequest>
    {
        public QuotationLineRequestValidator()
        {
            RuleFor(request => request.LineType)
                .NotEmpty()
                .Must(BeValidLineType)
                .WithMessage("Quotation line type is invalid.");
            RuleFor(request => request.LineDescription).NotEmpty().MaximumLength(256);
            RuleFor(request => request.Quantity).GreaterThan(0.00m);
            RuleFor(request => request.UnitPrice).GreaterThanOrEqualTo(0.00m);
        }

        private static bool BeValidLineType(string lineType)
        {
            return Enum.TryParse<QuotationLineType>(lineType, true, out _);
        }
    }
}
