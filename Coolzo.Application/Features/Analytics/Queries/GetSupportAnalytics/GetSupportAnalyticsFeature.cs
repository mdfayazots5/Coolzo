using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetSupportAnalytics;

public sealed record GetSupportAnalyticsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy,
    string? Status) : IRequest<SupportAnalyticsResponse>;

public sealed class GetSupportAnalyticsQueryValidator : AbstractValidator<GetSupportAnalyticsQuery>
{
    public GetSupportAnalyticsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<SupportTicketStatus>(status, true, out _))
            .WithMessage("Status must be a valid support ticket status.");
    }
}

public sealed class GetSupportAnalyticsQueryHandler : IRequestHandler<GetSupportAnalyticsQuery, SupportAnalyticsResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetSupportAnalyticsQueryHandler> _logger;

    public GetSupportAnalyticsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetSupportAnalyticsQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<SupportAnalyticsResponse> Handle(GetSupportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            null,
            _currentDateTime.UtcNow);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? (SupportTicketStatus?)null
            : Enum.Parse<SupportTicketStatus>(request.Status, true);
        var readModel = await _analyticsReadRepository.GetSupportAnalyticsAsync(filter, (int?)status, cancellationToken);

        _logger.LogInformation(
            "Support analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToSupportAnalytics(readModel);
    }
}

