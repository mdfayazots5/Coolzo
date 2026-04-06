namespace Coolzo.Application.Common.Models;

public sealed record AnalyticsQueryFilter(
    DateOnly DateFrom,
    DateOnly DateTo,
    string TrendBy,
    long? ServiceId,
    long? TechnicianId)
{
    public static AnalyticsQueryFilter Create(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? trendBy,
        long? serviceId,
        long? technicianId,
        DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);
        var normalizedDateTo = dateTo ?? today;
        var normalizedDateFrom = dateFrom ?? normalizedDateTo.AddDays(-29);

        if (dateFrom is not null && dateTo is null)
        {
            normalizedDateTo = normalizedDateFrom.AddDays(29);
        }

        if (dateFrom is null && dateTo is not null)
        {
            normalizedDateFrom = normalizedDateTo.AddDays(-29);
        }

        if (normalizedDateTo > today)
        {
            normalizedDateTo = today;
        }

        if (normalizedDateFrom > normalizedDateTo)
        {
            normalizedDateFrom = normalizedDateTo;
        }

        return new AnalyticsQueryFilter(
            normalizedDateFrom,
            normalizedDateTo,
            NormalizeTrendBy(trendBy),
            serviceId,
            technicianId);
    }

    private static string NormalizeTrendBy(string? trendBy)
    {
        if (string.Equals(trendBy, "week", StringComparison.OrdinalIgnoreCase))
        {
            return "week";
        }

        if (string.Equals(trendBy, "month", StringComparison.OrdinalIgnoreCase))
        {
            return "month";
        }

        return "day";
    }
}

