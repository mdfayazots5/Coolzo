using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestList;

public sealed record GetServiceRequestListQuery(
    long? BookingId,
    long? ServiceId,
    string? Status,
    DateOnly? SlotDate,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<ServiceRequestListItemResponse>>;
