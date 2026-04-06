using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.ChecklistResponse.Queries.GetJobChecklist;

public sealed class GetJobChecklistQueryHandler : IRequestHandler<GetJobChecklistQuery, IReadOnlyCollection<JobChecklistItemResponse>>
{
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;

    public GetJobChecklistQueryHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IFieldLookupRepository fieldLookupRepository)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<IReadOnlyCollection<JobChecklistItemResponse>> Handle(GetJobChecklistQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestAsync(request.ServiceRequestId, cancellationToken);
        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();

        return TechnicianJobResponseMapper.ToChecklistItems(serviceRequest, checklistMasters);
    }
}
