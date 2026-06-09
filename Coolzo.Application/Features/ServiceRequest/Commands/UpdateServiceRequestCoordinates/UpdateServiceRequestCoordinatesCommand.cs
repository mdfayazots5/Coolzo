using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.UpdateServiceRequestCoordinates;

public sealed record UpdateServiceRequestCoordinatesCommand(
    long ServiceRequestId,
    double Latitude,
    double Longitude) : IRequest<ServiceRequestDetailResponse>;
