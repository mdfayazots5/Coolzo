namespace Coolzo.Contracts.Responses.Billing;

public sealed record BillingStatusResponse(
    long InvoiceId,
    string InvoiceNumber,
    string InvoiceStatus,
    decimal GrandTotalAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    IReadOnlyCollection<BillingStatusHistoryResponse> Timeline);
