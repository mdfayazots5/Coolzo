namespace Coolzo.Contracts.Responses.Inventory;

public sealed record JobPartConsumptionSummaryResponse(
    long JobCardId,
    string JobCardNumber,
    long? TechnicianId,
    string? TechnicianName,
    int TotalLines,
    decimal TotalQuantityUsed,
    decimal TotalAmount,
    IReadOnlyCollection<JobPartConsumptionResponse> Items);
