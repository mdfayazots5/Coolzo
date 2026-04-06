namespace Coolzo.Contracts.Responses.Inventory;

public sealed record JobPartConsumptionResponse(
    long JobPartConsumptionId,
    long JobCardId,
    string JobCardNumber,
    long TechnicianId,
    string TechnicianName,
    long ItemId,
    string ItemCode,
    string ItemName,
    decimal QuantityUsed,
    decimal UnitPrice,
    decimal LineAmount,
    DateTime ConsumedDateUtc,
    string ConsumptionRemarks);
