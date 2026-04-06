namespace Coolzo.Domain.Entities;

public sealed record InventoryAnalyticsReadModel(
    long TotalItems,
    long LowStockItems,
    decimal TotalOnHandQuantity,
    decimal ConsumedQuantity,
    IReadOnlyCollection<LowStockInventoryItemReadModel> LowStockSummaries,
    IReadOnlyCollection<InventoryConsumptionTrendPointReadModel> ConsumptionTrends);

