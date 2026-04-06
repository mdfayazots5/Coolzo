using System.Text;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Reporting.Queries.GetReportExport;

public sealed record GetReportExportQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy,
    string? Format) : IRequest<ReportExportResponse>;

public sealed class GetReportExportQueryValidator : AbstractValidator<GetReportExportQuery>
{
    public GetReportExportQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");

        RuleFor(request => request.Format)
            .Must(format => string.IsNullOrWhiteSpace(format)
                || string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase)
                || string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Format must be csv or json.");
    }
}

public sealed class GetReportExportQueryHandler : IRequestHandler<GetReportExportQuery, ReportExportResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetReportExportQueryHandler> _logger;

    public GetReportExportQueryHandler(
        IMediator mediator,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetReportExportQueryHandler> logger)
    {
        _mediator = mediator;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<ReportExportResponse> Handle(GetReportExportQuery request, CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(
            new GetReportByDateRange.GetReportByDateRangeQuery(request.DateFrom, request.DateTo, request.TrendBy),
            cancellationToken);
        var format = string.Equals(request.Format, "json", StringComparison.OrdinalIgnoreCase) ? "json" : "csv";
        var generatedAt = _currentDateTime.UtcNow;
        var fileName = $"coolzo-analytics-report-{generatedAt:yyyyMMddHHmmss}.{format}";
        var content = format == "json"
            ? System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
            : BuildCsv(report);

        _logger.LogInformation("Report export generated in {Format} format.", format);

        return new ReportExportResponse(
            format,
            fileName,
            format == "json" ? "application/json" : "text/csv",
            content,
            generatedAt.ToString("O"));
    }

    private static string BuildCsv(DateRangeReportResponse report)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Section,Metric,Value");
        builder.AppendLine($"Summary,Date From,{report.DateFrom}");
        builder.AppendLine($"Summary,Date To,{report.DateTo}");
        builder.AppendLine($"Summary,Total Bookings,{report.TotalBookings}");
        builder.AppendLine($"Summary,Total Revenue,{report.TotalRevenue}");
        builder.AppendLine($"Summary,Completed Jobs,{report.CompletedJobs}");
        builder.AppendLine($"Summary,Support Tickets,{report.TotalSupportTickets}");
        builder.AppendLine($"Summary,Active Technicians,{report.ActiveTechnicians}");
        builder.AppendLine($"Summary,New Customers,{report.NewCustomers}");

        foreach (var trend in report.BookingTrends)
        {
            builder.AppendLine($"Booking Trend,{trend.PeriodLabel},{trend.Value}");
        }

        foreach (var trend in report.RevenueTrends)
        {
            builder.AppendLine($"Revenue Trend,{trend.PeriodLabel},{trend.Value}");
        }

        foreach (var item in report.SupportStatusDistribution)
        {
            builder.AppendLine($"Support Status,{item.Label},{item.Value}");
        }

        return builder.ToString();
    }
}
