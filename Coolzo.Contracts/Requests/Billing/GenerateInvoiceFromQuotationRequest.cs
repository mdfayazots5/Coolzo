namespace Coolzo.Contracts.Requests.Billing;

public sealed record GenerateInvoiceFromQuotationRequest(
    decimal DiscountAmount,
    decimal TaxPercentage,
    string? Remarks);
