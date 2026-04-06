namespace Coolzo.Domain.Entities;

public sealed record AnalyticsTrendPointReadModel(
    DateOnly PeriodStartDate,
    string PeriodLabel,
    decimal Value);

public sealed record AnalyticsBreakdownItemReadModel(
    string Label,
    decimal Value);

public sealed record TechnicianPerformanceItemReadModel(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    long JobsAssigned,
    long JobsCompleted,
    decimal CompletionRatePercentage,
    decimal AverageCompletionHours,
    long CurrentWorkload);

public sealed record CustomerTrendPointReadModel(
    DateOnly PeriodStartDate,
    string PeriodLabel,
    long NewCustomers,
    long ReturningCustomers);

public sealed record SupportResolutionTrendPointReadModel(
    DateOnly PeriodStartDate,
    string PeriodLabel,
    long ResolvedTickets,
    decimal AverageResolutionHours);

public sealed record LowStockInventoryItemReadModel(
    long ItemId,
    string ItemCode,
    string ItemName,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    decimal ShortageQuantity);

public sealed record InventoryConsumptionTrendPointReadModel(
    DateOnly PeriodStartDate,
    string PeriodLabel,
    decimal QuantityConsumed);

