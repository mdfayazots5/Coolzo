using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Models;

public sealed record BillingCalculationResult(
    decimal SubTotalAmount,
    decimal DiscountAmount,
    decimal TaxPercentage,
    decimal TaxAmount,
    decimal GrandTotalAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    InvoicePaymentStatus InvoicePaymentStatus);
