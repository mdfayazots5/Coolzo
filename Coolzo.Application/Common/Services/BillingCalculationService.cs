using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Services;

public sealed class BillingCalculationService : IBillingCalculationService
{
    public decimal CalculateLineAmount(decimal quantity, decimal unitPrice)
    {
        return RoundCurrency(quantity * unitPrice);
    }

    public BillingCalculationResult CalculateQuotation(IReadOnlyCollection<BillingLineCalculationInput> lines)
    {
        var subtotalAmount = CalculateSubTotal(lines);

        return new BillingCalculationResult(
            subtotalAmount,
            0.00m,
            0.00m,
            0.00m,
            subtotalAmount,
            0.00m,
            subtotalAmount,
            InvoicePaymentStatus.Unpaid);
    }

    public BillingCalculationResult CalculateInvoice(
        IReadOnlyCollection<BillingLineCalculationInput> lines,
        decimal discountAmount,
        decimal taxPercentage,
        decimal paidAmount)
    {
        var subtotalAmount = CalculateSubTotal(lines);
        var normalizedDiscountAmount = RoundCurrency(Math.Max(0.00m, discountAmount));
        var taxableAmount = Math.Max(0.00m, subtotalAmount - normalizedDiscountAmount);
        var normalizedTaxPercentage = Math.Max(0.00m, taxPercentage);
        var taxAmount = RoundCurrency(taxableAmount * normalizedTaxPercentage / 100.00m);
        var grandTotalAmount = RoundCurrency(taxableAmount + taxAmount);
        var normalizedPaidAmount = RoundCurrency(Math.Max(0.00m, paidAmount));
        var balanceAmount = CalculateBalanceAmount(grandTotalAmount, normalizedPaidAmount);

        return new BillingCalculationResult(
            subtotalAmount,
            normalizedDiscountAmount,
            normalizedTaxPercentage,
            taxAmount,
            grandTotalAmount,
            normalizedPaidAmount,
            balanceAmount,
            ResolvePaymentStatus(grandTotalAmount, normalizedPaidAmount));
    }

    public InvoicePaymentStatus ResolvePaymentStatus(decimal grandTotalAmount, decimal paidAmount)
    {
        if (paidAmount <= 0.00m)
        {
            return InvoicePaymentStatus.Unpaid;
        }

        return paidAmount >= grandTotalAmount
            ? InvoicePaymentStatus.Paid
            : InvoicePaymentStatus.PartiallyPaid;
    }

    public decimal CalculateBalanceAmount(decimal grandTotalAmount, decimal paidAmount)
    {
        return RoundCurrency(Math.Max(0.00m, grandTotalAmount - paidAmount));
    }

    private static decimal CalculateSubTotal(IReadOnlyCollection<BillingLineCalculationInput> lines)
    {
        return RoundCurrency(lines.Sum(line => line.Quantity * line.UnitPrice));
    }

    private static decimal RoundCurrency(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
