namespace Coolzo.Contracts.Responses.Billing;

public sealed record InvoiceListItemResponse(
    long InvoiceId,
    string InvoiceNumber,
    long QuotationId,
    string QuotationNumber,
    string CustomerName,
    string CurrentStatus,
    decimal GrandTotalAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    DateTime InvoiceDateUtc);
