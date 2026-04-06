namespace Coolzo.Contracts.Responses.Amc;

public sealed record CustomerAmcResponse(
    long CustomerAmcId,
    long CustomerId,
    string CustomerName,
    long AmcPlanId,
    string PlanName,
    long JobCardId,
    string JobCardNumber,
    long InvoiceId,
    string InvoiceNumber,
    string CurrentStatus,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    int TotalVisitCount,
    int ConsumedVisitCount,
    decimal PriceAmount,
    IReadOnlyCollection<AmcVisitScheduleResponse> Visits);
