using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Domain.Enums;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobList;

public sealed class GetTechnicianJobListQueryHandler : IRequestHandler<GetTechnicianJobListQuery, PagedResult<TechnicianJobListItemResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetTechnicianJobListQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        ITechnicianJobAccessService technicianJobAccessService,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _technicianJobAccessService = technicianJobAccessService;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
    }

    public async Task<PagedResult<TechnicianJobListItemResponse>> Handle(GetTechnicianJobListQuery request, CancellationToken cancellationToken)
    {
        var technician = await _technicianJobAccessService.GetCurrentTechnicianAsync(cancellationToken);
        var status = ParseStatus(request.Status);
        var jobs = await _serviceRequestRepository.SearchAssignedJobsAsync(
            technician.TechnicianId,
            status,
            request.SlotDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var totalCount = await _serviceRequestRepository.CountAssignedJobsAsync(
            technician.TechnicianId,
            status,
            request.SlotDate,
            cancellationToken);

        var items = await Task.WhenAll(
            jobs.Select(async job =>
            {
                var (lifecycleType, lifecycleLabel) = await _technicianJobLifecycleResolver.ResolveAsync(job.ServiceRequestId, cancellationToken);
                return TechnicianJobResponseMapper.ToListItem(job, lifecycleType, lifecycleLabel);
            }));

        return new PagedResult<TechnicianJobListItemResponse>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static ServiceRequestStatus? ParseStatus(string? status)
    {
        return Enum.TryParse<ServiceRequestStatus>(status, true, out var parsedStatus)
            ? parsedStatus
            : null;
    }
}
