using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.OperationsDashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, OperationsDashboardSummaryResponse>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetDashboardSummaryQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<OperationsDashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _serviceRequestRepository.GetDashboardSummaryAsync(cancellationToken);

        return new OperationsDashboardSummaryResponse(
            summary.TotalBookings,
            summary.TotalServiceRequests,
            summary.AssignedServiceRequests,
            summary.UnassignedServiceRequests,
            summary.EnRouteCount,
            summary.ReachedCount,
            summary.WorkStartedCount,
            summary.WorkInProgressCount,
            summary.SubmittedForClosureCount,
            summary.ActiveTechnicianCount,
            summary.TechnicianMonitoring
                .Select(item => new TechnicianMonitoringItemResponse(
                    item.TechnicianId,
                    item.TechnicianCode,
                    item.TechnicianName,
                    item.TodayAssignedJobsCount,
                    item.ActiveJobsCount,
                    item.CurrentActiveJobNumber,
                    item.CurrentActiveStatus))
                .ToArray());
    }
}
