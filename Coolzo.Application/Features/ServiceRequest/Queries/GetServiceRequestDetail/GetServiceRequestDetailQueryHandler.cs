using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestDetail;

public sealed class GetServiceRequestDetailQueryHandler : IRequestHandler<GetServiceRequestDetailQuery, ServiceRequestDetailResponse>
{
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetServiceRequestDetailQueryHandler(
        IServiceRequestRepository serviceRequestRepository,
        IFieldLookupRepository fieldLookupRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<ServiceRequestDetailResponse> Handle(GetServiceRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);
        var serviceId = serviceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();

        return ServiceRequestResponseMapper.ToDetail(serviceRequest, checklistMasters);
    }
}
