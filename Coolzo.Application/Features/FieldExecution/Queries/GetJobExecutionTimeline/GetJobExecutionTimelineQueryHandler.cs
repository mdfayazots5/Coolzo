using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Queries.GetJobExecutionTimeline;

public sealed class GetJobExecutionTimelineQueryHandler : IRequestHandler<GetJobExecutionTimelineQuery, IReadOnlyCollection<JobExecutionTimelineItemResponse>>
{
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetJobExecutionTimelineQueryHandler(ITechnicianJobAccessService technicianJobAccessService)
    {
        _technicianJobAccessService = technicianJobAccessService;
    }

    public async Task<IReadOnlyCollection<JobExecutionTimelineItemResponse>> Handle(GetJobExecutionTimelineQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        return TechnicianJobResponseMapper.ToExecutionTimeline(serviceRequest);
    }
}
