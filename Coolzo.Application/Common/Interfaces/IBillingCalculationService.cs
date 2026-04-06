using Coolzo.Application.Common.Models;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Interfaces;

public interface IBillingCalculationService
{
    decimal CalculateLineAmount(decimal quantity, decimal unitPrice);

    BillingCalculationResult CalculateQuotation(IReadOnlyCollection<BillingLineCalculationInput> lines);

    BillingCalculationResult CalculateInvoice(
        IReadOnlyCollection<BillingLineCalculationInput> lines,
        decimal discountAmount,
        decimal taxPercentage,
        decimal paidAmount);

    InvoicePaymentStatus ResolvePaymentStatus(decimal grandTotalAmount, decimal paidAmount);

    decimal CalculateBalanceAmount(decimal grandTotalAmount, decimal paidAmount);
}
