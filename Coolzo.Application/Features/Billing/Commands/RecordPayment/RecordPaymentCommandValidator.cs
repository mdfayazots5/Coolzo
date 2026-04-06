using Coolzo.Domain.Enums;
using FluentValidation;

namespace Coolzo.Application.Features.Billing.Commands.RecordPayment;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(request => request.InvoiceId).GreaterThan(0);
        RuleFor(request => request.PaidAmount).GreaterThan(0.00m);
        RuleFor(request => request.PaymentMethod)
            .NotEmpty()
            .Must(BeValidPaymentMethod)
            .WithMessage("Payment method is invalid.");
        RuleFor(request => request.ReferenceNumber).MaximumLength(128);
        RuleFor(request => request.Remarks).MaximumLength(512);
        RuleFor(request => request.IdempotencyKey).MaximumLength(128);
        RuleFor(request => request.GatewayTransactionId).MaximumLength(128);
        RuleFor(request => request.Signature).MaximumLength(256);
        RuleFor(request => request.ExpectedInvoiceAmount).GreaterThan(0.00m).When(request => request.ExpectedInvoiceAmount.HasValue);
        RuleFor(request => request.WebhookReference).MaximumLength(128);
    }

    private static bool BeValidPaymentMethod(string paymentMethod)
    {
        return Enum.TryParse<PaymentMethod>(paymentMethod, true, out _);
    }
}
