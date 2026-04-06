namespace Coolzo.Contracts.Requests.Inventory;

public sealed record ConsumeJobPartItemRequest(
    long ItemId,
    decimal QuantityUsed,
    string? ConsumptionRemarks);
