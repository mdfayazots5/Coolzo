namespace Coolzo.Contracts.Requests.Billing;

public sealed record CreateQuotationFromJobRequest(
    IReadOnlyCollection<QuotationLineRequest> Lines,
    decimal DiscountAmount,
    decimal TaxPercentage,
    string? Remarks);
