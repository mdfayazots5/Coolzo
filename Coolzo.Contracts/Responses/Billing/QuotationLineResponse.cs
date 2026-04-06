namespace Coolzo.Contracts.Responses.Billing;

public sealed record QuotationLineResponse(
    long QuotationLineId,
    string LineType,
    string LineDescription,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineAmount);
