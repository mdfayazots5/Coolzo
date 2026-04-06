using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetTechnicianPerformance;

public sealed record GetTechnicianPerformanceQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy,
    long? TechnicianId,
    string? Status) : IRequest<TechnicianPerformanceResponse>;

public sealed class GetTechnicianPerformanceQueryValidator : AbstractValidator<GetTechnicianPerformanceQuery>
{
    public GetTechnicianPerformanceQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");

        RuleFor(request => request.TechnicianId)
            .GreaterThan(0)
            .When(request => request.TechnicianId.HasValue);

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<ServiceRequestStatus>(status, true, out _))
            .WithMessage("Status must be a valid service request status.");
    }
}

public sealed class GetTechnicianPerformanceQueryHandler : IRequestHandler<GetTechnicianPerformanceQuery, TechnicianPerformanceResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetTechnicianPerformanceQueryHandler> _logger;

    public GetTechnicianPerformanceQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetTechnicianPerformanceQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<TechnicianPerformanceResponse> Handle(GetTechnicianPerformanceQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            request.TechnicianId,
            _currentDateTime.UtcNow);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? (ServiceRequestStatus?)null
            : Enum.Parse<ServiceRequestStatus>(request.Status, true);
        var readModel = await _analyticsReadRepository.GetTechnicianPerformanceAsync(filter, (int?)status, cancellationToken);

        _logger.LogInformation(
            "Technician performance analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToTechnicianPerformance(readModel);
    }
}

