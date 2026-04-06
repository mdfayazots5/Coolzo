using Coolzo.Contracts.Responses.TechnicianJobs;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobReached;

public sealed record MarkTechnicianJobReachedCommand(
    long ServiceRequestId,
    string? Remarks,
    string? WorkSummary) : IRequest<TechnicianJobDetailResponse>;
