using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobEnRoute;

public sealed record MarkTechnicianJobEnRouteCommand(
    long ServiceRequestId,
    string? Remarks,
    string? WorkSummary) : IRequest<TechnicianJobDetailResponse>;
