using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestDetail;

public sealed record GetServiceRequestDetailQuery(
    long ServiceRequestId) : IRequest<ServiceRequestDetailResponse>;
