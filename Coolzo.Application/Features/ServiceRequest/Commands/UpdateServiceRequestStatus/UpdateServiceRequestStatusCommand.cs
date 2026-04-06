using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestStatus;

public sealed record UpdateServiceRequestStatusCommand(
    long ServiceRequestId,
    string Status,
    string? Remarks) : IRequest<ServiceRequestDetailResponse>;
