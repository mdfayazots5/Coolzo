using Coolzo.Contracts.Responses.Operations;
using MediatR;

namespace Coolzo.Application.Features.Assignment.Commands.ReassignTechnician;

public sealed record ReassignTechnicianCommand(
    long ServiceRequestId,
    long TechnicianId,
    string? Remarks) : IRequest<ServiceRequestDetailResponse>;
