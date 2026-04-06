using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Enums;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestList;

public sealed class GetServiceRequestListQueryHandler : IRequestHandler<GetServiceRequestListQuery, PagedResult<ServiceRequestListItemResponse>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetServiceRequestListQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<PagedResult<ServiceRequestListItemResponse>> Handle(GetServiceRequestListQuery request, CancellationToken cancellationToken)
    {
        var currentStatus = ParseStatus(request.Status);
        var serviceRequests = await _serviceRequestRepository.SearchAsync(
            request.BookingId,
            request.ServiceId,
            currentStatus,
            request.SlotDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _serviceRequestRepository.CountSearchAsync(
            request.BookingId,
            request.ServiceId,
            currentStatus,
            request.SlotDate,
            cancellationToken);

        return new PagedResult<ServiceRequestListItemResponse>(
            serviceRequests.Select(ServiceRequestResponseMapper.ToListItem).ToArray(),
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
