namespace Coolzo.Application.Common.Validation;

public static class AnalyticsValidationRules
{
    public static bool HasValidDateRange(DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (!dateFrom.HasValue || !dateTo.HasValue)
        {
            return true;
        }

        return dateFrom.Value <= dateTo.Value
            && dateTo.Value.DayNumber - dateFrom.Value.DayNumber <= 366;
    }

    public static bool HasValidTrendBy(string? trendBy)
    {
        return string.IsNullOrWhiteSpace(trendBy)
            || string.Equals(trendBy, "day", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trendBy, "week", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trendBy, "month", StringComparison.OrdinalIgnoreCase);
    }
}

