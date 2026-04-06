using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Assignment.Commands.AssignTechnician;

public sealed record AssignTechnicianCommand(
    long ServiceRequestId,
    long? TechnicianId,
    string? Remarks) : IRequest<ServiceRequestDetailResponse>;
