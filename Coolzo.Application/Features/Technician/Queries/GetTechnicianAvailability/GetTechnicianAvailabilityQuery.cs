using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Technician.Queries.GetTechnicianAvailability;

public sealed record GetTechnicianAvailabilityQuery(
    long ServiceRequestId) : IRequest<IReadOnlyCollection<TechnicianAvailabilityResponse>>;
