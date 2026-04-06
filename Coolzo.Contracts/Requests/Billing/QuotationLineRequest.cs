namespace Coolzo.Contracts.Requests.Billing;

public sealed record QuotationLineRequest(
    string LineType,
    string LineDescription,
    decimal Quantity,
    decimal UnitPrice);
