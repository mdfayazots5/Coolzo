using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Reporting.Queries.GetReportByDateRange;

public sealed record GetReportByDateRangeQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy) : IRequest<DateRangeReportResponse>;

public sealed class GetReportByDateRangeQueryValidator : AbstractValidator<GetReportByDateRangeQuery>
{
    public GetReportByDateRangeQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");
    }
}

public sealed class GetReportByDateRangeQueryHandler : IRequestHandler<GetReportByDateRangeQuery, DateRangeReportResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetReportByDateRangeQueryHandler> _logger;

    public GetReportByDateRangeQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetReportByDateRangeQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<DateRangeReportResponse> Handle(GetReportByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            null,
            _currentDateTime.UtcNow);
        var readModel = await _analyticsReadRepository.GetReportByDateRangeAsync(filter, cancellationToken);

        _logger.LogInformation(
            "Date range report fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToDateRangeReport(readModel);
    }
}

