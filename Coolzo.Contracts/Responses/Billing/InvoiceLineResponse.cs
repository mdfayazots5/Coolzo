namespace Coolzo.Contracts.Responses.Billing;

public sealed record InvoiceLineResponse(
    long InvoiceLineId,
    long? QuotationLineId,
    string LineType,
    string LineDescription,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineAmount);
