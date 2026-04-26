using Coolzo.Application.Features.Analytics.Queries.GetRevenueAnalytics;
using Coolzo.Application.Features.Billing.Queries.GetAccountsReceivableDashboard;
using Coolzo.Application.Features.Reporting.Queries.GetReportByDateRange;
using Coolzo.Application.Features.Reporting.Queries.GetReportExport;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Policy = PermissionNames.ReportRead)]
[Route("api/reports")]
public sealed class ReportController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReportController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("finance-dashboard")]
    [ProducesResponseType(typeof(ApiResponse<FinanceDashboardKpiResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FinanceDashboardKpiResponse>>> GetFinanceDashboardAsync(
        CancellationToken cancellationToken)
    {
        var revenue = await _sender.Send(new GetRevenueAnalyticsQuery(null, null, "monthly", null), cancellationToken);
        var receivables = await _sender.Send(new GetAccountsReceivableDashboardQuery(), cancellationToken);
        var now = DateTime.UtcNow;

        var response = new FinanceDashboardKpiResponse(
            revenue.RevenueTrends.LastOrDefault()?.Value ?? 0m,
            revenue.RevenueTrends.TakeLast(7).Sum(item => item.Value),
            revenue.RevenueTrends.TakeLast(1).Sum(item => item.Value),
            revenue.TotalRevenue,
            revenue.TotalRevenue <= 0 ? 0m : Math.Round((revenue.PaidRevenue / revenue.TotalRevenue) * 100m, 2),
            receivables.TotalOutstanding,
            7,
            receivables.OverdueInvoices.Count(item =>
            {
                if (!DateTime.TryParse(item.DueDate, out var dueDateUtc))
                {
                    return false;
                }

                return dueDateUtc.Date == now.Date;
            }));

        return Success(response);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetRevenueAsync(
        [FromQuery] string? period,
        [FromQuery] string? breakdown,
        CancellationToken cancellationToken)
    {
        var trendBy = string.IsNullOrWhiteSpace(period) ? "monthly" : period;
        var revenue = await _sender.Send(new GetRevenueAnalyticsQuery(null, null, trendBy, null), cancellationToken);

        if (string.Equals(breakdown, "serviceType", StringComparison.OrdinalIgnoreCase))
        {
            var breakdownResponse = revenue.RevenueByService
                .Select(item => new FinanceRevenueBreakdownResponse(item.Label, item.Value))
                .ToArray();

            return Success<object>(breakdownResponse);
        }

        var trendResponse = revenue.RevenueTrends
            .Select(item => new FinanceRevenueTrendResponse(
                item.PeriodLabel,
                item.Value,
                Math.Round(item.Value * 1.08m, 2)))
            .ToArray();

        return Success<object>(trendResponse);
    }

    [HttpGet("collection-efficiency")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<FinanceCollectionEfficiencyResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<FinanceCollectionEfficiencyResponse>>>> GetCollectionEfficiencyAsync(
        CancellationToken cancellationToken)
    {
        var revenue = await _sender.Send(new GetRevenueAnalyticsQuery(null, null, "monthly", null), cancellationToken);
        var receivables = await _sender.Send(new GetAccountsReceivableDashboardQuery(), cancellationToken);
        var totalRevenue = revenue.TotalRevenue <= 0 ? 1m : revenue.TotalRevenue;
        var collectionRate = Math.Round((revenue.PaidRevenue / totalRevenue) * 100m, 2);
        var outstandingRatio = Math.Round((revenue.OutstandingRevenue / totalRevenue) * 100m, 2);
        var currentBucket = receivables.Aging.FirstOrDefault(item => string.Equals(item.Label, "0-30 Days", StringComparison.OrdinalIgnoreCase))?.Amount ?? 0m;
        var overdueBucket = receivables.Aging
            .Where(item => !string.Equals(item.Label, "0-30 Days", StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Amount);
        var totalOutstanding = receivables.TotalOutstanding <= 0 ? 1m : receivables.TotalOutstanding;

        var response = new[]
        {
            new FinanceCollectionEfficiencyResponse("Collection Rate", collectionRate),
            new FinanceCollectionEfficiencyResponse("Current Bucket Recovery", Math.Round((currentBucket / totalOutstanding) * 100m, 2)),
            new FinanceCollectionEfficiencyResponse("Overdue Exposure", Math.Round((overdueBucket / totalOutstanding) * 100m, 2)),
            new FinanceCollectionEfficiencyResponse("Outstanding Ratio", outstandingRatio),
        };

        return Success<IReadOnlyCollection<FinanceCollectionEfficiencyResponse>>(response);
    }

    [HttpGet("date-range")]
    [ProducesResponseType(typeof(ApiResponse<DateRangeReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DateRangeReportResponse>>> GetDateRangeAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetReportByDateRangeQuery(dateFrom, dateTo, trendBy),
            cancellationToken);

        return Success(response);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(ApiResponse<ReportExportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReportExportResponse>>> ExportAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? trendBy,
        [FromQuery] string? format,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetReportExportQuery(dateFrom, dateTo, trendBy, format),
            cancellationToken);

        return Success(response);
    }
}

public sealed record FinanceDashboardKpiResponse(
    decimal RevenueToday,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    decimal RevenueYtd,
    decimal CollectionRate,
    decimal OutstandingReceivables,
    int AvgDaysToCollect,
    int NewInvoicesToday);

public sealed record FinanceRevenueTrendResponse(
    string Period,
    decimal Amount,
    decimal Target);

public sealed record FinanceRevenueBreakdownResponse(
    string Type,
    decimal Amount);

public sealed record FinanceCollectionEfficiencyResponse(
    string Label,
    decimal Value);
