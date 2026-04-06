using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Queries.GetTechnicianJobDetail;

public sealed class GetTechnicianJobDetailQueryHandler : IRequestHandler<GetTechnicianJobDetailQuery, TechnicianJobDetailResponse>
{
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetTechnicianJobDetailQueryHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
    }

    public async Task<TechnicianJobDetailResponse> Handle(GetTechnicianJobDetailQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();
        var (lifecycleType, lifecycleLabel) = await _technicianJobLifecycleResolver.ResolveAsync(serviceRequest.ServiceRequestId, cancellationToken);
        var supportJobAlert = await _supportTicketRepository.GetJobAlertAsync(
            serviceRequest.ServiceRequestId,
            serviceRequest.BookingId,
            serviceRequest.JobCard?.JobCardId,
            cancellationToken);

        return TechnicianJobResponseMapper.ToDetail(serviceRequest, checklistMasters, lifecycleType, lifecycleLabel, supportJobAlert);
    }
}
