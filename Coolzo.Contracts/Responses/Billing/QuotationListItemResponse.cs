namespace Coolzo.Contracts.Responses.Billing;

public sealed record QuotationListItemResponse(
    long QuotationId,
    string QuotationNumber,
    long JobCardId,
    string JobCardNumber,
    long ServiceRequestId,
    string ServiceRequestNumber,
    string CustomerName,
    string CurrentStatus,
    decimal GrandTotalAmount,
    DateTime QuotationDateUtc);
