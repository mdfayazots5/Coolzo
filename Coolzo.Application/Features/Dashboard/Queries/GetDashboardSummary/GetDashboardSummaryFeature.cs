using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Analytics;
using MediatR;

namespace Coolzo.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery() : IRequest<DashboardSummaryResponse>;

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;

    public GetDashboardSummaryQueryHandler(IAnalyticsReadRepository analyticsReadRepository)
    {
        _analyticsReadRepository = analyticsReadRepository;
    }

    public async Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var readModel = await _analyticsReadRepository.GetDashboardSummaryAsync(cancellationToken);

        return AnalyticsResponseMapper.ToDashboardSummary(readModel);
    }
}

