using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.ServiceRequest.Commands.CreateServiceRequestFromBooking;

public sealed record CreateServiceRequestFromBookingCommand(
    long BookingId) : IRequest<ServiceRequestDetailResponse>;
