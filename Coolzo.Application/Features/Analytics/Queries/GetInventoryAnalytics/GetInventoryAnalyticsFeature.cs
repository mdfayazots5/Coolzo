using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetInventoryAnalytics;

public sealed record GetInventoryAnalyticsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy) : IRequest<InventoryAnalyticsResponse>;

public sealed class GetInventoryAnalyticsQueryValidator : AbstractValidator<GetInventoryAnalyticsQuery>
{
    public GetInventoryAnalyticsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");
    }
}

public sealed class GetInventoryAnalyticsQueryHandler : IRequestHandler<GetInventoryAnalyticsQuery, InventoryAnalyticsResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetInventoryAnalyticsQueryHandler> _logger;

    public GetInventoryAnalyticsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetInventoryAnalyticsQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<InventoryAnalyticsResponse> Handle(GetInventoryAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            null,
            _currentDateTime.UtcNow);
        var readModel = await _analyticsReadRepository.GetInventoryAnalyticsAsync(filter, cancellationToken);

        _logger.LogInformation(
            "Inventory analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToInventoryAnalytics(readModel);
    }
}

