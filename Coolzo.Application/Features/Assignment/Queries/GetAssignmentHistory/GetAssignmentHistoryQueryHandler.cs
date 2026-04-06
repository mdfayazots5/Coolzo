using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.ServiceRequest;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Assignment.Queries.GetAssignmentHistory;

public sealed class GetAssignmentHistoryQueryHandler : IRequestHandler<GetAssignmentHistoryQuery, IReadOnlyCollection<AssignmentHistoryItemResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetAssignmentHistoryQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<IReadOnlyCollection<AssignmentHistoryItemResponse>> Handle(GetAssignmentHistoryQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        return ServiceRequestResponseMapper.ToAssignmentHistory(serviceRequest);
    }
}
