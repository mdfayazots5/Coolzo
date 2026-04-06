namespace Coolzo.Contracts.Requests.Amc;

public sealed record AssignAmcToCustomerRequest(
    long CustomerId,
    long AmcPlanId,
    long JobCardId,
    long InvoiceId,
    DateTime? StartDateUtc,
    string? Remarks);
